using TreeNodeWebApi.Models.DTOs.Tree;

namespace TreeNodeWebApi.Interfaces
{   
    public interface ITreeService
    {
        Task<TreeNodeDto> GetNodeAsync(int id);
        Task<List<TreeNodeDto>> GetRootNodesAsync();
        Task<TreeNodeDto> CreateNodeAsync(CreateTreeNodeRequest request);
        Task<TreeNodeDto> UpdateNodeAsync(int id, UpdateTreeNodeRequest request);
        Task DeleteNodeAsync(int id);
        Task<TreeNodeDto> MoveNodeAsync(int nodeId, int? newParentId);
        Task<TreeExportDto> ExportTreeAsync();
        Task<bool> CheckForCyclesAsync(int nodeId, int? newParentId);
    }
}
