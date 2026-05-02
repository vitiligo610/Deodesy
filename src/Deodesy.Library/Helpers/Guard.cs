using System;

namespace Deodesy.Library.Helpers
{
    /// <summary>
    /// Provides common guard clauses to validate method arguments.
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the specified argument is null.
        /// </summary>
        /// <param name="value">The argument to check for null.</param>
        /// <param name="parameterName">The name of the parameter that is being checked.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <example>
        /// <code>
        /// public void MyMethod(object myObject)
        /// {
        ///     Guard.NotNull(myObject, nameof(myObject));
        ///     // ... rest of method
        /// }
        /// </code>
        /// </example>
        public static void NotNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the specified string argument is null or empty.
        /// </summary>
        /// <param name="value">The string argument to check for null or empty.</param>
        /// <param name="parameterName">The name of the parameter that is being checked.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null or an empty string.</exception>
        /// <example>
        /// <code>
        /// public void MyMethod(string myString)
        /// {
        ///     Guard.NotNullOrEmpty(myString, nameof(myString));
        ///     // ... rest of method
        /// }
        /// </code>
        /// </example>
        public static void NotNullOrEmpty(string value, string parameterName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}