namespace GhaithAI.API.GaithAI.Domain.Common
{
    /// <summary>
    /// Abstract base class for all domain entities with strongly-typed identifiers.
    /// Provides a generic foundation for entity identity and creation tracking.
    /// </summary>
    /// <typeparam name="TId">The type of the entity's primary key (e.g., Guid, string, int).</typeparam>
    public abstract class BaseEntity<TId>
        where TId : notnull
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEntity{TId}"/> class.
        /// </summary>
        protected BaseEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseEntity{TId}"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The primary key value.</param>
        protected BaseEntity(TId id)
        {
            Id = id;
        }

        /// <summary>
        /// The primary key of the entity. Must be a non-null value type.
        /// </summary>
        public TId Id { get; set; }

        /// <summary>
        /// The date and time (UTC) when this entity was created.
        /// Automatically initialized to the current UTC time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
