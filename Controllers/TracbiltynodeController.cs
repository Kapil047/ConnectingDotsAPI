using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; 
using System.Data.SqlClient;
using System.Data; 
using ConnectingDotsAPI.Models;
using Dapper;

namespace ConnectingDotsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TracbiltynodeController : ControllerBase
    {
   
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public TracbiltynodeController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("CS");
        }

        // सिंगल नोड सेव करने का API
        [HttpPost("node")]
        [Authorize]
        public async Task<IActionResult> SaveNode([FromBody] TraceabilityNodeModel node)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Guid", node.Guid ?? Guid.NewGuid().ToString());
                    parameters.Add("@ParentNodeId", node.ParentNodeId);
                    parameters.Add("@OrderItemGuid", node.OrderItemGuid);
                    parameters.Add("@Name", node.Name);
                    parameters.Add("@Type", node.Type);
                    parameters.Add("@Process", node.Process);
                    parameters.Add("@SupplierId", node.SupplierId);
                    parameters.Add("@SubContractor", node.SubContractor);
                    parameters.Add("@DisplayOrder", node.DisplayOrder);

                    // Check if node already exists
                    var existingNode = await connection.QueryFirstOrDefaultAsync<TraceabilityNodeModel>(
                        "SELECT * FROM dbo.TraceabilityNode WHERE Guid = @Guid",
                        new { node.Guid });

                    if (existingNode != null)
                    {
                        // Update existing node
                        await connection.ExecuteAsync(@"
                            UPDATE dbo.TraceabilityNode 
                            SET Name = @Name,
                                Type = @Type,
                                Process = @Process,
                                SupplierId = @SupplierId,
                                SubContractor = @SubContractor,
                                DisplayOrder = @DisplayOrder,
                                UpdatedOnUtc = GETUTCDATE()
                            WHERE Guid = @Guid",
                            parameters);

                        return Ok(new
                        {
                            success = true,
                            message = "Node updated successfully",
                            guid = node.Guid,
                            id = existingNode.Id
                        });
                    }
                    else
                    {
                        // Insert new node
                        var result = await connection.QuerySingleAsync<TraceabilityNodeModel>(@"
                            INSERT INTO dbo.TraceabilityNode 
                            (Guid, ParentNodeId, OrderItemGuid, Name, Type, Process, SupplierId, SubContractor, DisplayOrder)
                            VALUES 
                            (@Guid, @ParentNodeId, @OrderItemGuid, @Name, @Type, @Process, @SupplierId, @SubContractor, @DisplayOrder);
                            SELECT * FROM dbo.TraceabilityNode WHERE Guid = @Guid",
                            parameters);

                        return Ok(new
                        {
                            success = true,
                            message = "Node saved successfully",
                            guid = result.Guid,
                            id = result.Id
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // मल्टीपल नोड्स सेव करने का API
        [HttpPost("nodes/batch")]
        [Authorize]
        public async Task<IActionResult> SaveNodesBatch([FromBody] List<TraceabilityNodeModel> nodes)
        {
            try
            {
                var results = new List<object>();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            foreach (var node in nodes)
                            {
                                var parameters = new DynamicParameters();
                                parameters.Add("@Guid", node.Guid ?? Guid.NewGuid().ToString());
                                parameters.Add("@ParentNodeId", node.ParentNodeId);
                                parameters.Add("@OrderItemGuid", node.OrderItemGuid);
                                parameters.Add("@Name", node.Name);
                                parameters.Add("@Type", node.Type);
                                parameters.Add("@Process", node.Process);
                                parameters.Add("@SupplierId", node.SupplierId);
                                parameters.Add("@SubContractor", node.SubContractor);
                                parameters.Add("@DisplayOrder", node.DisplayOrder);

                                var existingNode = await connection.QueryFirstOrDefaultAsync<TraceabilityNodeModel>(
                                    "SELECT * FROM dbo.TraceabilityNode WHERE Guid = @Guid",
                                    new { node.Guid }, transaction);

                                if (existingNode != null)
                                {
                                    await connection.ExecuteAsync(@"
                                        UPDATE dbo.TraceabilityNode 
                                        SET Name = @Name,
                                            Type = @Type,
                                            Process = @Process,
                                            SupplierId = @SupplierId,
                                            SubContractor = @SubContractor,
                                            DisplayOrder = @DisplayOrder,
                                            UpdatedOnUtc = GETUTCDATE()
                                        WHERE Guid = @Guid",
                                        parameters, transaction);
                                }
                                else
                                {
                                    await connection.ExecuteAsync(@"
                                        INSERT INTO dbo.TraceabilityNode 
                                        (Guid, ParentNodeId, OrderItemGuid, Name, Type, Process, SupplierId, SubContractor, DisplayOrder)
                                        VALUES 
                                        (@Guid, @ParentNodeId, @OrderItemGuid, @Name, @Type, @Process, @SupplierId, @SubContractor, @DisplayOrder)",
                                        parameters, transaction);
                                }

                                results.Add(new
                                {
                                    guid = node.Guid,
                                    name = node.Name,
                                    status = "saved"
                                });
                            }

                            transaction.Commit();

                            return Ok(new
                            {
                                success = true,
                                message = $"{nodes.Count} nodes saved successfully",
                                results
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // फाइल सेव करने का API
        [HttpPost("file")]
        [Authorize]
        public async Task<IActionResult> SaveFile([FromBody] TraceabilityFileModel file)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // First get NodeId using Guid
                    var node = await connection.QueryFirstOrDefaultAsync<TraceabilityNodeModel>(
                        "SELECT Id FROM dbo.TraceabilityNode WHERE Guid = @Guid",
                        new { file.NodeGuid });

                    if (node == null)
                        return BadRequest(new { success = false, message = "Node not found" });

                    var parameters = new DynamicParameters();
                    parameters.Add("@NodeId", node.Id);
                    parameters.Add("@FileName", file.FileName);
                    parameters.Add("@FileUrl", file.FileUrl);
                    parameters.Add("@FileSize", file.FileSize);
                    parameters.Add("@FileType", file.FileType);

                    var result = await connection.ExecuteAsync(@"
                        INSERT INTO dbo.TraceabilityFile 
                        (NodeId, FileName, FileUrl, FileSize, FileType)
                        VALUES 
                        (@NodeId, @FileName, @FileUrl, @FileSize, @FileType)",
                        parameters);

                    return Ok(new { success = true, message = "File saved successfully" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // OrderItem के सभी नोड्स लाने का API
        [HttpGet("orderitem/{orderItemGuid}")]
        public async Task<IActionResult> GetNodesByOrderItem(string orderItemGuid)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var nodes = await connection.QueryAsync<TraceabilityNodeModel>(@"
                        SELECT * FROM dbo.TraceabilityNode 
                        WHERE OrderItemGuid = @OrderItemGuid AND IsActive = 1
                        ORDER BY DisplayOrder",
                        new { OrderItemGuid = Guid.Parse(orderItemGuid) });

                    return Ok(new { success = true, nodes });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        // Delete node by GUID
        [HttpDelete("node/{guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteNode(string guid)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    // Check if node exists
                    var node = await connection.QueryFirstOrDefaultAsync<TraceabilityNodeModel>(
                        "SELECT * FROM dbo.TraceabilityNode WHERE Guid = @Guid",
                        new { Guid = guid });

                    if (node == null)
                        return BadRequest(new { success = false, message = "Node not found" });

                    // Delete the node (and children if cascade delete is set)
                    var result = await connection.ExecuteAsync(
                        "DELETE FROM dbo.TraceabilityNode WHERE Guid = @Guid",
                        new { Guid = guid });

                    return Ok(new { success = true, message = "Node deleted successfully" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Tree structure लाने का API (parent-child hierarchy)
        [HttpGet("tree/{orderItemGuid}")]
        public async Task<IActionResult> GetTree(string orderItemGuid)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var nodes = await connection.QueryAsync<TraceabilityNodeModel>(@"
                        SELECT * FROM dbo.TraceabilityNode 
                        WHERE OrderItemGuid = @OrderItemGuid AND IsActive = 1",
                        new { OrderItemGuid = Guid.Parse(orderItemGuid) });

                    // Build tree structure
                    var tree = BuildTree(nodes.ToList(), null);

                    return Ok(new { success = true, tree });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private List<TraceabilityNodeModel> BuildTree(List<TraceabilityNodeModel> nodes, Guid? parentId)
        {
            return nodes
                .Where(n => n.ParentNodeId == parentId)
                .Select(n => {
                    n.Components = BuildTree(nodes, n.Id);
                    return n;
                })
                .ToList();
        }
    }

    // MODELS
    public class TraceabilityNodeModel
    {
        public Guid Id { get; set; }
        public string Guid { get; set; }
        public Guid? ParentNodeId { get; set; }
        public Guid OrderItemGuid { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Process { get; set; }
        public Guid? SupplierId { get; set; }
        public bool SubContractor { get; set; }
        public int DisplayOrder { get; set; }
        public List<TraceabilityNodeModel> Components { get; set; } = new List<TraceabilityNodeModel>();
        public List<TraceabilityFileModel> Files { get; set; } = new List<TraceabilityFileModel>();
    }

    public class TraceabilityFileModel
    {
        public Guid Id { get; set; }
        public string NodeGuid { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public long? FileSize { get; set; }
        public string FileType { get; set; }
    }
}