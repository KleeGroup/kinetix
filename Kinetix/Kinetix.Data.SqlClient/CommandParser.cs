using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Threading;
using Kinetix.Security;
using Microsoft.SqlServer.Server;

namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// Analyseur de requête SQL Dynamique.
    ///
    /// Syntaxe :
    ///   [include]Querie1[/include]                        : insère le contenu de la ressource Querie1 définie dans le fichier de resource DalIncludedQueries.resx (avant toute autre opération de parsing).
    ///                                                       ou tout autre fichier de ressource enregistré par  SqlServerManager.Instance.RegisterIncludeQueryResource depuis KinetixApplication.InitSqlManager.
    /// - #MaxRows#                                         : insère la valeur de MaxRows de la commande; doit être utilisé pour la gestion des TOP.
    /// - {CurrentUserId}                                   : crée un paramètre prenant la valeur de l'utilisateur courant.
    /// - {CurrentUserProfile}                              : crée un paramètre prenant la valeur du code du profil de l'utilisateur courant.
    /// - {Type.Constante}                                  : crée un paramètre prenant la valeur de la constante "Constante" portée par le type "Type".
    /// - [if null="PARAM1"]...[/if]                        : insère le contenu de la balise si PARAM1 est nul.
    /// - [if null="PARAM1&amp;PARAM2"]...[/if]             : insère le contenu de la balise si PARAM1 est nul et PARAM2 est nul.
    /// - [if null="PARAM1|PARAM2"]...[/if]                 : insère le contenu de la balise si PARAM1 est nul ou PARAM2 est nul.
    /// - [if notnull="PARAM1"]...[/if]                     : insère le contenu de la balise si PARAM1 est non nul.
    /// - [if notnull="PARAM1&amp;PARAM2"]...[/if]          : insère le contenu de la balise si PARAM1 est non nul et PARAM2 est non nul.
    /// - [if notnull="PARAM1|PARAM2"]...[/if]              : insère le contenu de la balise si PARAM1 est non nul ou PARAM2 est non nul.
    /// - [if equals="PARAM1:2"]...[/if]                    : insère le contenu de la balise si PARAM1 est égal à 2.
    /// - [if equals="PARAM1:2:3"]...[/if]                  : insère le contenu de la balise si PARAM1 est égal à 2 ou 3.
    /// - [if equals="PARAM1:{Type.Constante}"]...[/if]     : insère le contenu de la balise si PARAM1 est égal à la valeur de la constante "Constante" portée par le type "Type".
    /// - [if equals="{IsInRole}:RW"]...[/if]               : insère le contenu de la balise si l'utilisateur courant a pour droit une des valeurs.
    /// - [if notequals="PARAM1:2"]...[/if]                 : insère le contenu de la balise si PARAM1 est différent de 2.
    /// - [if notequals="PARAM1:2:3"]...[/if]               : insère le contenu de la balise si PARAM1 est différent de 2 et 3.
    /// - [if notequals="PARAM1:{Type.Constante}"]...[/if]  : insère le contenu de la balise si PARAM1 est différent de la valeur de la constante "Constante" portée par le type "Type".
    /// - [if notequals="{IsInRole}:RW"]...[/if]            : insère le contenu de la balise si l'utilisateur courant n'a pas pour droit une des valeurs.
    /// </summary>
    internal static class CommandParser {
        private const string TagIfStart = "[if ";
        private const string TagIfEnd = "[/if]";

        private const string AttributeEquals = "equals";
        private const string AttributeNotEquals = "notequals";
        private const string AttributeNull = "null";
        private const string AttributeNotNull = "notnull";

        private const string TagStaticParStart = "{";
        private const string TagStaticParEnd = "}";
        private const string CurrentUserIdParameterName = "CurrentUserId";
        private const string CurrentUserProfileParamaterName = "CurrentUserProfile";
        private const string IsRegularUserParameterName = "IsRegularUser";
        private const string IsInRoleParamaterName = "IsInRole";
        private const string OrderParameterName = "Order";
        private const string MaxRows = "#MaxRows#";
        private const string Offset = "#Offset#";

        private const string TagIncludeStart = "[include]";
        private const string TagIncludeEnd = "[/include]";

        private static readonly Dictionary<string, bool> _keyTable = new Dictionary<string, bool>();
        private static readonly object _keyTableLock = new object();

        /// <summary>
        /// Type de jeton.
        /// </summary>
        private enum TokenType {

            /// <summary>
            /// Instruction IF.
            /// </summary>
            IfStatement,

            /// <summary>
            /// Paramétre UserId.
            /// </summary>
            CurrentUserIdParameter,

            /// <summary>
            /// Paramètre User.CodeProfil.
            /// </summary>
            CurrentUserProfileParameter,

            /// <summary>
            /// Constante.
            /// </summary>
            Constant,

            /// <summary>
            /// Order clause.
            /// </summary>
            Order
        }

        /// <summary>
        /// Parse une commande pour gérer les requêtes dynamiques.
        /// </summary>
        /// <param name="command">Commande.</param>
        /// <param name="parserKey">Clef représentative de la commande.</param>
        /// <param name="maxRows">MaxRows à injecter.</param>
        /// <param name="queryParameter">Paramètres de la requête (limit, offset, tri).</param>
        internal static void ParseCommand(IDbCommand command, string parserKey, int? maxRows, QueryParameter queryParameter = null) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            if (parserKey == null) {
                return;
            }

            if (command.CommandType != CommandType.Text) {
                return;
            }

            bool isParsingNeeded;
            bool hasKey;
            lock (_keyTableLock) {
                hasKey = _keyTable.TryGetValue(parserKey, out isParsingNeeded);
            }

            if (hasKey) {
                if (!isParsingNeeded) {
                    RemoveUnusedParameters(command);
                    return;
                }
            }

            string commandText = InsertIncludeQueries(command.CommandText);
            StringBuilder sqlBuilder = new StringBuilder(commandText.Length);

            int currentPos = 0;
            Token t = GetNextToken(commandText, currentPos);

            if (t != null) {
                while (t != null) {
                    sqlBuilder.Append(commandText.Substring(currentPos, t.Position - currentPos));
                    currentPos = ProcessToken(command, commandText, sqlBuilder, t, true, queryParameter);
                    t = GetNextToken(commandText, currentPos);
                }
            }

            sqlBuilder.Append(commandText.Substring(currentPos));

            command.CommandText = sqlBuilder.ToString();
            if (queryParameter != null) {
                if (queryParameter.Limit > 0) {
                    command.CommandText = command.CommandText.Replace(MaxRows, queryParameter.Limit.ToString(CultureInfo.InvariantCulture));
                }

                if (queryParameter.Offset > 0) {
                    command.CommandText = command.CommandText.Replace(Offset, queryParameter.Offset.ToString(CultureInfo.InvariantCulture));
                }
            }

            if (!isParsingNeeded) {
                bool needParsing = !commandText.Equals(command.CommandText);
                lock (_keyTableLock) {
                    _keyTable[parserKey] = needParsing;
                }
            }

            RemoveUnusedParameters(command);
        }

        /// <summary>
        /// Supprime les paramètres inutilisés de la commande.
        /// </summary>
        /// <param name="command">Commande.</param>
        private static void RemoveUnusedParameters(IDbCommand command) {
            List<IDataParameter> unusedParameters = new List<IDataParameter>();
            IDataParameterCollection parameters = command.Parameters;

            string commandText = command.CommandText;
            foreach (IDataParameter param in parameters) {
                if (commandText.IndexOf(param.ParameterName, StringComparison.OrdinalIgnoreCase) < 0) {
                    unusedParameters.Add(param);
                }
            }

            foreach (IDataParameter param in unusedParameters) {
                parameters.Remove(param);
            }
        }

        /// <summary>
        /// Traite un jeton.
        /// </summary>
        /// <param name="command">Commande.</param>
        /// <param name="commandText">Texte de la commande.</param>
        /// <param name="sqlBuilder">Buffer SQL.</param>
        /// <param name="t">Jeton.</param>
        /// <param name="isExpressionEnabled">Indique si l'expression en cours est active.</param>
        /// <param name="queryParameter">Paramètres de tri et de pagination.</param>
        /// <returns>Prochaine position.</returns>
        private static int ProcessToken(IDbCommand command, string commandText, StringBuilder sqlBuilder, Token t, bool isExpressionEnabled, QueryParameter queryParameter) {
            switch (t.Type) {
                case TokenType.IfStatement:
                    return ParseExpression(sqlBuilder, command, commandText, t.Position, isExpressionEnabled, queryParameter) + 1;
                case TokenType.CurrentUserIdParameter:
                    if (isExpressionEnabled) {
                        sqlBuilder.Append("@CURRENT_USER_ID");
                        if (!command.Parameters.Contains("@CURRENT_USER_ID")) {
                            IDbDataParameter parameter = command.CreateParameter();
                            parameter.ParameterName = "@CURRENT_USER_ID";
                            var userId = StandardUser.UserId;
                            if (userId != null) {
                                parameter.Value = userId.Value;
                            } else {
                                parameter.Value = 0;
                            }

                            command.Parameters.Add(parameter);
                        }
                    }

                    return t.EndPosition + 1;
                case TokenType.Constant:
                    if (isExpressionEnabled) {
                        sqlBuilder.Append(t.ConstantValue);
                    }

                    return t.EndPosition + 1;
                case TokenType.Order:
                    if (isExpressionEnabled) {
                        sqlBuilder.Append(GenerateOrderByClause(queryParameter));
                    }

                    return t.EndPosition + 1;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Create orderby clause.
        /// </summary>
        /// <param name="queryParameter">Query parameter.</param>
        /// <returns>Order by clause.</returns>
        private static string GenerateOrderByClause(QueryParameter queryParameter) {
            if (queryParameter != null) {
                string sortCondition = queryParameter.SortCondition;
                if (!string.IsNullOrEmpty(sortCondition)) {
                    return " order by " + sortCondition;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Retourne l'index de la prochaine instruction.
        /// </summary>
        /// <param name="commandText">Texte de la commande SQL.</param>
        /// <param name="currentPos">Position courante.</param>
        /// <returns>Position de la prochaine instruction.</returns>
        private static Token GetNextToken(string commandText, int currentPos) {
            int ifPos = commandText.IndexOf(TagIfStart, currentPos, StringComparison.OrdinalIgnoreCase);
            int cstPos = commandText.IndexOf(TagStaticParStart, currentPos, StringComparison.OrdinalIgnoreCase);

            if (cstPos >= 0 && (cstPos < ifPos || ifPos < 0)) {
                int cstEndPos = commandText.IndexOf(TagStaticParEnd, cstPos, StringComparison.OrdinalIgnoreCase);
                if (cstEndPos >= 0) {
                    string constant = commandText.Substring(cstPos + 1, cstEndPos - cstPos - 1);
                    if (CurrentUserIdParameterName.Equals(constant)) {
                        return new Token { Position = cstPos, Type = TokenType.CurrentUserIdParameter, EndPosition = cstEndPos };
                    }

                    if (CurrentUserProfileParamaterName.Equals(constant)) {
                        return new Token { Position = cstPos, Type = TokenType.CurrentUserProfileParameter, EndPosition = cstEndPos };
                    }

                    if (OrderParameterName.Equals(constant)) {
                        return new Token { Position = cstPos, Type = TokenType.Order, EndPosition = cstEndPos };
                    }

                    if (constant.Split('.').Length == 2) {
                        object constantValue = SqlServerManager.Instance.GetConstValueByShortName(constant);
                        if (constantValue != null) {
                            string stringValue = constantValue.ToString();
                            if (stringValue != null) {
                                stringValue = "'" + stringValue.Replace("'", "''") + "'";
                            } else {
                                throw new NotImplementedException(constantValue.GetType().FullName);
                            }

                            return new Token { Position = cstPos, Type = TokenType.Constant, EndPosition = cstEndPos, ConstantValue = stringValue };
                        }
                    }
                }
            }

            return ifPos >= 0 ? new Token { Position = ifPos, Type = TokenType.IfStatement } : null;
        }

        /// <summary>
        /// Parse une expression.
        /// </summary>
        /// <param name="sqlBuilder">Buffer SQL.</param>
        /// <param name="command">Commande SQL.</param>
        /// <param name="commandText">Texte complet de la commande initiale.</param>
        /// <param name="index">Position courant du parseur dans le texte de la commande.</param>
        /// <param name="isOutputEnabled">Indique si l'expression est valide.</param>
        /// <param name="queryParameter">Paramètre de tri et de pagination.</param>
        /// <returns>Position de sortie.</returns>
        private static int ParseExpression(StringBuilder sqlBuilder, IDbCommand command, string commandText, int index, bool isOutputEnabled, QueryParameter queryParameter) {
            int currentPos = index;

            currentPos = commandText.IndexOf("]", currentPos, StringComparison.OrdinalIgnoreCase) + 1;
            if (currentPos < 1) {
                throw new NotSupportedException();
            }

            bool isExpressionEnabled = isOutputEnabled &&
                    IsExpressionEnabled(command.Parameters, commandText, index, currentPos);

            Token t = GetNextToken(commandText, currentPos);
            int nextClose = commandText.IndexOf(TagIfEnd, currentPos, StringComparison.OrdinalIgnoreCase);

            while (t != null && t.Position < nextClose) {
                if (isExpressionEnabled) {
                    sqlBuilder.Append(commandText.Substring(currentPos, t.Position - currentPos));
                }

                currentPos = ProcessToken(command, commandText, sqlBuilder, t, isExpressionEnabled, queryParameter);
                t = GetNextToken(commandText, currentPos);
                nextClose = commandText.IndexOf(TagIfEnd, currentPos, StringComparison.OrdinalIgnoreCase);
            }

            if (isExpressionEnabled) {
                sqlBuilder.Append(commandText.Substring(currentPos, nextClose - currentPos));
            }

            return nextClose + 4;
        }

        /// <summary>
        /// Indique si une expression est active.
        /// </summary>
        /// <param name="parameters">Listes des paramètres de la commande.</param>
        /// <param name="commandText">Texte de la commande.</param>
        /// <param name="startPos">Position de départ de la balise d'ouverture.</param>
        /// <param name="endPos">Position de fin de la balise d'ouverture.</param>
        /// <returns>True si le bloc de texte est actif.</returns>
        private static bool IsExpressionEnabled(IDataParameterCollection parameters, string commandText, int startPos, int endPos) {
            string[] array = commandText.Substring(startPos + 4, endPos - startPos - 4).Split('=', '"');

            if (AttributeNotNull.Equals(array[0])) {
                return CheckNull(parameters, array[2], false);
            }

            if (AttributeNull.Equals(array[0])) {
                return CheckNull(parameters, array[2], true);
            }

            if (AttributeEquals.Equals(array[0])) {
                return CheckEquals(parameters, array[2], true);
            }

            if (AttributeNotEquals.Equals(array[0])) {
                return CheckEquals(parameters, array[2], false);
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Vérifie si un paramètre est égal à une valeur.
        /// </summary>
        /// <param name="parameters">Liste des paramètres.</param>
        /// <param name="expression">Chaîne de caractères courante.</param>
        /// <param name="checkEquals">True si l'égalité est testée, false sinon.</param>
        /// <returns>True ou false.</returns>
        private static bool CheckEquals(IDataParameterCollection parameters, string expression, bool checkEquals) {
            string[] array = expression.Split(':');
            string paramName = array[0].Trim();

            if (paramName == TagStaticParStart + IsInRoleParamaterName + TagStaticParEnd) {
                for (int i = 1; i < array.Length; i++) {
                    string compareValue = ExtractDynamicValue(array[i]);
                    if (Thread.CurrentPrincipal.IsInRole(compareValue)) {
                        return checkEquals;
                    }
                }

                return !checkEquals;
            }

            if (paramName == TagStaticParStart + IsRegularUserParameterName + TagStaticParEnd) {
                return StandardUser.IsRegularUser;
            }

            object value = ((IDbDataParameter)parameters["@" + paramName]).Value;
            if (DBNull.Value.Equals(value)) {
                return !checkEquals;
            }

            string strValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            for (int i = 1; i < array.Length; i++) {
                string compareValue = ExtractDynamicValue(array[i]);
                if (strValue.Equals(compareValue)) {
                    return checkEquals;
                }
            }

            return !checkEquals;
        }

        /// <summary>
        /// Extrait d'une chaîne de caractère sa valeur dynamique (utilisation d'un paramètre, de CurrentUserProfile, etc).
        /// </summary>
        /// <param name="value">Chaîne d'entrée.</param>
        /// <returns>Valeur finale de la chaîne.</returns>
        private static string ExtractDynamicValue(string value) {
            if (value.StartsWith(TagStaticParStart, StringComparison.Ordinal) && value.EndsWith(TagStaticParEnd, StringComparison.Ordinal)) {
                string constant = value.Substring(1, value.Length - 2);
                if (CurrentUserIdParameterName.Equals(constant)) {
                    value = StandardUser.UserId.Value.ToString(CultureInfo.InvariantCulture);
                } else {
                    object constantValue = SqlServerManager.Instance.GetConstValueByShortName(constant);
                    if (constantValue == null) {
                        throw new NotSupportedException();
                    }

                    value = Convert.ToString(constantValue, CultureInfo.InvariantCulture);
                }
            }

            return value;
        }

        /// <summary>
        /// Vérifie si un paramètre est null ou non.
        /// </summary>
        /// <param name="parameters">Liste des paramètres.</param>
        /// <param name="expression">Chaîne de caractères courante.</param>
        /// <param name="checkNull">True si la nullité doit être testée, False pour tester la non-nullité.</param>
        /// <returns>True ou false.</returns>
        private static bool CheckNull(IDataParameterCollection parameters, string expression, bool checkNull) {
            bool containsAnd = expression.IndexOf("&", StringComparison.OrdinalIgnoreCase) >= 0;
            bool containsOr = expression.IndexOf("|", StringComparison.OrdinalIgnoreCase) >= 0;

            if (containsOr && containsAnd) {
                throw new NotSupportedException();
            }

            if (containsAnd) {
                return ParseExpression('&', parameters, expression, checkNull);
            }

            if (containsOr) {
                return ParseExpression('|', parameters, expression, checkNull);
            }

            try {
                bool isNull;
                SqlParameter parameter = (SqlParameter)parameters["@" + expression];
                if (parameter.SqlDbType != SqlDbType.Structured) {
                    isNull = DBNull.Value.Equals(parameter.Value);
                } else {
                    IList<SqlDataRecord> listValue = (IList<SqlDataRecord>)parameter.Value;
                    isNull = listValue.Count == 1 && listValue[0][0] == DBNull.Value;
                }

                return checkNull ? isNull : !isNull;
            } catch (KeyNotFoundException) {
                throw new NotSupportedException("Aucun paramétre du nom @" + expression + " n'existe pour cette commande");
            }
        }

        /// <summary>
        /// Parse une exception.
        /// </summary>
        /// <param name="oper">Opérateur.</param>
        /// <param name="parameters">Liste des paramètres.</param>
        /// <param name="expression">Chaîne de caractères courante.</param>
        /// <param name="checkNull">True si la nullité doit être testée, False pour tester la non-nullité.</param>
        /// <returns>True ou false.</returns>
        private static bool ParseExpression(char oper, IDataParameterCollection parameters, string expression, bool checkNull) {
            string[] paramArray = expression.Split(oper);
            for (int i = 0; i < paramArray.Length; i++) {
                string parameter = paramArray[i];
                if (!string.IsNullOrEmpty(parameter)) {
                    bool isNull = DBNull.Value.Equals(
                            ((IDbDataParameter)parameters["@" + parameter.Trim()]).Value);
                    if ('&'.Equals(oper) && (checkNull ? !isNull : isNull)) {
                        return false;
                    }

                    if ('|'.Equals(oper) && (checkNull ? isNull : !isNull)) {
                        return true;
                    }
                }
            }

            return '&'.Equals(oper);
        }

        /// <summary>
        /// Récupère les queries incluses dans la commande.
        /// </summary>
        /// <param name="commandText">La commande.</param>
        /// <returns>La commande avec les queries incluses dedant.</returns>
        private static string InsertIncludeQueries(string commandText) {
            StringBuilder sqlBuilder = new StringBuilder(commandText.Length);
            int currentPos = 0;
            int includePos = commandText.IndexOf(TagIncludeStart, currentPos, StringComparison.OrdinalIgnoreCase);
            while (includePos >= 0) {
                sqlBuilder.Append(commandText.Substring(currentPos, includePos - currentPos));
                currentPos = includePos + TagIncludeStart.Length;
                int endTagPos = commandText.IndexOf(TagIncludeEnd, currentPos, StringComparison.OrdinalIgnoreCase);
                if (endTagPos < 0) {
                    throw new NotSupportedException("le tag de fin de include n'a pas été trouvé");
                }

                string resourceNameToInclude = commandText.Substring(currentPos, endTagPos - currentPos).Trim();
                string queryToInclude = SqlServerManager.Instance.GetIncludeQuery(resourceNameToInclude);
                if (queryToInclude == null) {
                    throw new NotSupportedException("La ressource :" + resourceNameToInclude + " n'a pas été trouvée dans les fichiers de ressources enregistrés");
                }

                sqlBuilder.Append(queryToInclude);
                currentPos = endTagPos + TagIncludeEnd.Length;
                includePos = commandText.IndexOf(TagIncludeStart, currentPos, StringComparison.OrdinalIgnoreCase);
            }

            sqlBuilder.Append(commandText.Substring(currentPos));
            return sqlBuilder.ToString();
        }

        /// <summary>
        /// Jeton de position.
        /// </summary>
        private sealed class Token {
            /// <summary>
            /// Position.
            /// </summary>
            public int Position {
                get;
                set;
            }

            /// <summary>
            /// Type.
            /// </summary>
            public TokenType Type {
                get;
                set;
            }

            /// <summary>
            /// Position de fin.
            /// </summary>
            public int EndPosition {
                get;
                set;
            }

            /// <summary>
            /// Valeur de la constante.
            /// </summary>
            public string ConstantValue {
                get;
                set;
            }
        }
    }
}
