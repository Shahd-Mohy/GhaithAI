namespace GhaithAI.API.GaithAI.Domain.Common
{
    /// <summary>
    /// Abstract base class for domain entities requiring full audit trails and soft delete capabilities.
    /// Inherits from <see cref="BaseEntity{TId}"/> and implements <see cref="ISoftDelete"/> for compliance tracking.
    /// Essential for sensitive clinical/medical entities that require legal compliance and data retention policies.
    /// </summary>
    /// <typeparam name="TId">The type of the entity's primary key (e.g., Guid, string, int).</typeparam>
    public abstract class AuditableEntity<TId> : BaseEntity<TId>, ISoftDelete
        where TId : notnull
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditableEntity{TId}"/> class.
        /// </summary>
        protected AuditableEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditableEntity{TId}"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The primary key value.</param>
        protected AuditableEntity(TId id) : base(id)
        {
        }

        /// <summary>
        /// The date and time (UTC) when this entity was last updated.
        /// Null if the entity has never been modified after creation.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Identifier or name of the user or system that last modified this entity.
        /// Null if the entity has never been modified after creation.
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Indicates whether this entity has been soft-deleted.
        /// Required by <see cref="ISoftDelete"/> interface.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// The date and time (UTC) when this entity was soft-deleted.
        /// Required by <see cref="ISoftDelete"/> interface.
        /// Null if the entity has not been deleted.
        /// </summary>
        public DateTime? DeletedAt { get; set; }

    }
}
