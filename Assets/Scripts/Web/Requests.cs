using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Web
{
    public static class Requests
    {
        /////////
        // properties
        /////////

        public static System.Net.Http.HttpClient HttpClient = new System.Net.Http.HttpClient();

        public static string Address
        {
            get
            {
                return "http://" + IMP.IMPConfiguration.CollisionIP + ":" + IMP.IMPConfiguration.CollisionPort + "/";
            }
        }

        public static string Host
        {
            get
            {
                return IMP.IMPConfiguration.CollisionIP + ":" + IMP.IMPConfiguration.CollisionPort;
            }
        }

        /////////
        // tasks
        /////////

        public static async Task<int> CreateWebObject(
            bool movable,
            Vector3[] vertices,
            int[] triangles,
            Vector3 scale,
            IMP.Configuration configuration,
            string name
        )
        {
            string uri = Address + "create";

            float[] vertices_float = new float[vertices.Length * 3];
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices_float[3 * i + 0] = vertices[i].x * scale.x;
                vertices_float[3 * i + 1] = vertices[i].y * scale.y;
                vertices_float[3 * i + 2] = vertices[i].z * scale.z;
            }

            var request = new Web.DTO.ObjectCreationRequest()
            {
                movable = movable,
                vertices = vertices_float,
                triangles = triangles,
                position_x = configuration.Position.x,
                position_y = configuration.Position.y,
                position_z = configuration.Position.z,
                rotation_w = configuration.Rotation.w,
                rotation_x = configuration.Rotation.x,
                rotation_y = configuration.Rotation.y,
                rotation_z = configuration.Rotation.z,
            };

            var content = new System.Net.Http.StringContent(JsonUtility.ToJson(request));
            var result = await HttpClient.PutAsync(uri, content);
            result.EnsureSuccessStatusCode();
            var result_content = await result.Content.ReadAsStringAsync();

            var result_unpacked = JsonUtility.FromJson<Web.DTO.ObjectCreationResult>(result_content);
            if (result_unpacked.movable != movable)
                throw new Exception("Movable object creation request failed with static object!");

            if (name != null)
            {
                Tool.Console.Log("Object \"" + name + "\" initialized: " + (movable ? "M" : "S") + "-" + result_unpacked.id);
            }
            return result_unpacked.id;
        }

        public static async Task<Tool.Pair<bool, IMP.Configuration>> NewLocalClosestRemote(
            int WebId,
            IMP.Configuration start,
            IMP.Configuration end
        )
        {
            string uri = Web.Requests.Address + "new-local-closest";

            var request = new Web.DTO.NewLocalClosestRequest()
            {
                movable_id = WebId,

                start_position_x = start.Position.x,
                start_position_y = start.Position.y,
                start_position_z = start.Position.z,

                start_rotation_w = start.Rotation.w,
                start_rotation_x = start.Rotation.x,
                start_rotation_y = start.Rotation.y,
                start_rotation_z = start.Rotation.z,

                end_position_x = end.Position.x,
                end_position_y = end.Position.y,
                end_position_z = end.Position.z,

                end_rotation_w = end.Rotation.w,
                end_rotation_x = end.Rotation.x,
                end_rotation_y = end.Rotation.y,
                end_rotation_z = end.Rotation.z
            };

            var content = new System.Net.Http.StringContent(JsonUtility.ToJson(request));
            var result = await Web.Requests.HttpClient.PutAsync(uri, content);
            result.EnsureSuccessStatusCode();
            var result_content = await result.Content.ReadAsStringAsync();

            var result_unpacked = JsonUtility.FromJson<Web.DTO.NewLocalClosestResult>(result_content);

            if (!result_unpacked.successful)
            {

            }
            Tool.Pair<bool, IMP.Configuration> ret_val;
            ret_val.First = result_unpacked.successful;
            ret_val.Second = new IMP.Configuration()
            {
                Position = new Vector3(
                    result_unpacked.position_x,
                    result_unpacked.position_y,
                    result_unpacked.position_z
                ),
                Rotation = new Quaternion(
                    result_unpacked.rotation_x,
                    result_unpacked.rotation_y,
                    result_unpacked.rotation_z,
                    result_unpacked.rotation_w
                )
            };
            return ret_val;
        }

        public static async Task<bool> IsCollisionFreePathRemote(
            int WebId,
            IMP.Configuration start,
            IMP.Configuration end)
        {
            string uri = Address + "is-collision-free-path";

            var request = new Web.DTO.NewLocalClosestRequest()
            {
                movable_id = WebId,

                start_position_x = start.Position.x,
                start_position_y = start.Position.y,
                start_position_z = start.Position.z,

                start_rotation_w = start.Rotation.w,
                start_rotation_x = start.Rotation.x,
                start_rotation_y = start.Rotation.y,
                start_rotation_z = start.Rotation.z,

                end_position_x = end.Position.x,
                end_position_y = end.Position.y,
                end_position_z = end.Position.z,

                end_rotation_w = end.Rotation.w,
                end_rotation_x = end.Rotation.x,
                end_rotation_y = end.Rotation.y,
                end_rotation_z = end.Rotation.z
            };

            var content = new System.Net.Http.StringContent(JsonUtility.ToJson(request));
            var result = await HttpClient.PutAsync(uri, content);
            result.EnsureSuccessStatusCode();
            var result_content = await result.Content.ReadAsStringAsync();

            var result_unpacked = JsonUtility.FromJson<DTO.CollisionResult>(result_content);

            return !result_unpacked.is_colliding;
        }

        public static async Task<bool> MovedRequest(
            int WebId,
            IMP.Configuration start,
            IMP.Configuration end)
        {
            string uri = Address + "moved";

            var request = new DTO.MovedRequest()
            {
                movable_id = WebId,

                start_position_x = start.Position.x,
                start_position_y = start.Position.y,
                start_position_z = start.Position.z,

                start_rotation_w = start.Rotation.w,
                start_rotation_x = start.Rotation.x,
                start_rotation_y = start.Rotation.y,
                start_rotation_z = start.Rotation.z,

                end_position_x = end.Position.x,
                end_position_y = end.Position.y,
                end_position_z = end.Position.z,

                end_rotation_w = end.Rotation.w,
                end_rotation_x = end.Rotation.x,
                end_rotation_y = end.Rotation.y,
                end_rotation_z = end.Rotation.z
            };

            var content = new System.Net.Http.StringContent(JsonUtility.ToJson(request));
            var result = await HttpClient.PutAsync(uri, content);
            result.EnsureSuccessStatusCode();
            return true;
        }

        public static async Task<Tool.Pair<int, bool>> PathTo(
            int WebId,
            IMP.Configuration root,
            List<IMP.Configuration> matchees
        )
        {
            string uri = Address + "path-to";

            var request = new DTO.PathToRequest()
            {
                movable_id = WebId,

                start_position_x = root.Position.x,
                start_position_y = root.Position.y,
                start_position_z = root.Position.z,

                start_rotation_x = root.Rotation.x,
                start_rotation_y = root.Rotation.y,
                start_rotation_z = root.Rotation.z,
                start_rotation_w = root.Rotation.w,

                u_positions_x = matchees.Select(c => c.Position.x).ToArray(),
                u_positions_y = matchees.Select(c => c.Position.y).ToArray(),
                u_positions_z = matchees.Select(c => c.Position.z).ToArray(),

                u_rotations_x = matchees.Select(c => c.Rotation.x).ToArray(),
                u_rotations_y = matchees.Select(c => c.Rotation.y).ToArray(),
                u_rotations_z = matchees.Select(c => c.Rotation.z).ToArray(),
                u_rotations_w = matchees.Select(c => c.Rotation.w).ToArray()
            };

            var content = new System.Net.Http.StringContent(JsonUtility.ToJson(request));
            var result = await HttpClient.PutAsync(uri, content);
            result.EnsureSuccessStatusCode();
            var result_content = await result.Content.ReadAsStringAsync();

            var result_unpacked = JsonUtility.FromJson<DTO.PathToResult>(result_content);

            return new Tool.Pair<int, bool>()
            {
                First = result_unpacked.path_to_request_id,
                Second = result_unpacked.successful
            };
        }

        public static async Task<bool> PathToStatus(
            int path_to_request_id
        )
        {
            string uri = Address + "path-to-status";

            var request = new DTO.PathToStatusRequest()
            {
                path_to_request_id = path_to_request_id
            };

            var content = new System.Net.Http.StringContent(JsonUtility.ToJson(request));
            var result = await HttpClient.PutAsync(uri, content);
            result.EnsureSuccessStatusCode();
            var result_content = await result.Content.ReadAsStringAsync();

            var result_unpacked = JsonUtility.FromJson<DTO.PathToStatusResult>(result_content);

            return result_unpacked.finished;
        }

        public static async Task<Tool.Pair<int, List<IMP.Configuration>>> PathToGet(
            int path_to_request_id,
            int movable_id
        )
        {
            string uri = Address + "path-to-get";

            var request = new DTO.PathToGetRequest()
            {
                path_to_request_id = path_to_request_id,
                movable_id = movable_id
            };

            var content = new System.Net.Http.StringContent(JsonUtility.ToJson(request));
            var result = await HttpClient.PutAsync(uri, content);
            result.EnsureSuccessStatusCode();
            var result_content = await result.Content.ReadAsStringAsync();

            var result_unpacked = JsonUtility.FromJson<Web.DTO.PathToGetResult>(result_content);

            var configs = new List<IMP.Configuration>();
            for (int i = 0; i < result_unpacked.p_positions_x.Count(); ++i)
            {
                configs.Add(new IMP.Configuration()
                {
                    Position = new Vector3()
                    {
                        x = result_unpacked.p_positions_x[i],
                        y = result_unpacked.p_positions_y[i],
                        z = result_unpacked.p_positions_z[i],
                    },
                    Rotation = new Quaternion()
                    {
                        x = result_unpacked.p_rotations_x[i],
                        y = result_unpacked.p_rotations_y[i],
                        z = result_unpacked.p_rotations_z[i],
                        w = result_unpacked.p_rotations_w[i]
                    }
                });
            }

            return new Tool.Pair<int, List<IMP.Configuration>>()
            {
                First = result_unpacked.matchee_result,
                Second = configs
            };
        }

        public static async Task Trashes(
            params Tool.Pair<int, bool>[] to_delete
            )
        {
            string uri = Address + "trashes";

            var request = new DTO.ObjectsTrashRequest()
            {
                movables = to_delete.Select(x => x.Second).ToArray(),
                ids = to_delete.Select(x => x.First).ToArray()
            };

            var content = new System.Net.Http.StringContent(JsonUtility.ToJson(request));
            var result = await HttpClient.PutAsync(uri, content);
            result.EnsureSuccessStatusCode();
        }
    }
}