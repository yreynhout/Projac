using System;
using System.Data;
using System.Data.SqlClient;

namespace Projac
{
    /// <summary>
    ///     Represents the T-SQL UNIQUEIDENTIFIER NULL parameter value.
    /// </summary>
    public class TSqlUniqueIdentifierNullValue : ITSqlParameterValue
    {
        /// <summary>
        ///     The single instance of this value.
        /// </summary>
        public static readonly TSqlUniqueIdentifierNullValue Instance = new TSqlUniqueIdentifierNullValue();

        TSqlUniqueIdentifierNullValue() { }

        /// <summary>
        ///     Creates a <see cref="SqlParameter" /> instance based on this instance.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>
        ///     A <see cref="SqlParameter" />.
        /// </returns>
        public SqlParameter ToSqlParameter(string parameterName)
        {
            return new SqlParameter(
                parameterName,
                SqlDbType.UniqueIdentifier,
                16,
                ParameterDirection.Input,
                true,
                0,
                0,
                "",
                DataRowVersion.Default,
                DBNull.Value);
        }

        private static bool Equals(TSqlUniqueIdentifierNullValue value)
        {
            return ReferenceEquals(value, Instance);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;
            return Equals((TSqlUniqueIdentifierNullValue)obj);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}