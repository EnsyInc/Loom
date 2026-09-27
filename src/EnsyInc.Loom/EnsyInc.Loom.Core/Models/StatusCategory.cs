namespace EnsyInc.Loom.Core.Models;

public enum StatusCategory
{
    /// <summary>
    /// Work has not started yet.
    /// </summary>
    ToDo = 0,

    /// <summary>
    /// Work is underway.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Work is finished.
    /// </summary>
    Done = 2,
}
