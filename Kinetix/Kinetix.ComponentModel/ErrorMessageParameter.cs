namespace Kinetix.ComponentModel {
    /// <summary>
    /// Parameter of an error message sent from Server to client.
    /// </summary>
    public class ErrorMessageParameter {
        /// <summary>
        /// Value of the parameter.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Domain of the parameter.
        /// </summary>
        public string Domain { get; set; }
    }
}
