namespace GhaithAI.API.GaithAI.Domain.Common
{
    /// <summary>
    /// Interface for entities that support soft delete functionality.
    /// Soft deletes are essential for compliance and audit trails in clinical/sensitive data.
    /// </summary>
    public interface ISoftDelete
    {
        /// <summary>
        /// Indicates whether this entity has been soft-deleted.
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// The date and time (UTC) when this entity was soft-deleted.
        /// Null if the entity has not been deleted.
        /// </summary>
        DateTime? DeletedAt { get; set; }
    }
}
