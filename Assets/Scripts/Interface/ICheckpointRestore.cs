public interface ICheckpointRestore
{
    void SaveToCheckpoint(CheckpointData data);
    void LoadFromCheckpoint(CheckpointData data);    
}
