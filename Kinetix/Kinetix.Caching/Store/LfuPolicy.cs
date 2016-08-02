/*
 *  Copyright 2003-2007 Greg Luck
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;

namespace Kinetix.Caching.Store {
    /// <summary>
    /// Contains common LFU policy code for use between the LfuMemoryStore and the DiskStore, which also
    /// uses an LfuPolicy for evictions.
    /// </summary>
    internal static class LfuPolicy {
        private const int DefaultSampleSize = 30;
        private static readonly Random _random = new Random();

        /// <summary>
        /// Generates a random sample from a population.
        /// </summary>
        /// <param name="populationSize">The size to draw from.</param>
        /// <returns>Sample offsets.</returns>
        public static int[] GenerateRandomSample(int populationSize) {
            int sampleSize = CalculateSampleSize(populationSize);
            int[] offsets = new int[sampleSize];
            int maxOffset = populationSize / sampleSize;
            for (int i = 0; i < sampleSize; i++) {
                offsets[i] = _random.Next(maxOffset);
            }

            return offsets;
        }

        /// <summary>
        /// Finds the least hit of the sampled elements provided.
        /// </summary>
        /// <param name="sampledElements">This should be a random subset of the population.</param>
        /// <param name="justAdded">We never want to select the element just added. May be null.</param>
        /// <returns>The least hit.</returns>
        public static IMetaData LeastHit(IMetaData[] sampledElements, IMetaData justAdded) {
            if (sampledElements == null) {
                throw new ArgumentNullException("sampledElements");
            }

            // Edge condition when Memory Store configured to size 0
            if (sampledElements.Length == 1) {
                return justAdded;
            }

            IMetaData lowestElement = null;
            for (int i = 0; i < sampledElements.Length; i++) {
                IMetaData element = sampledElements[i];
                if (lowestElement == null) {
                    if (!element.Equals(justAdded)) {
                        lowestElement = element;
                    }
                } else {
                    if (element.HitCount < lowestElement.HitCount && !element.Equals(justAdded)) {
                        lowestElement = element;
                    }
                }
            }

            return lowestElement;
        }

        /// <summary>
        /// SampleSize how many samples to take.
        /// </summary>
        /// <param name="populationSize">The smaller of the map size and the default sample size of 30.</param>
        /// <returns>SampleSize.</returns>
        private static int CalculateSampleSize(int populationSize) {
            if (populationSize < DefaultSampleSize) {
                return populationSize;
            } else {
                return DefaultSampleSize;
            }
        }
    }
}
