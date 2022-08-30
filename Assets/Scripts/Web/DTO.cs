using System;

namespace Web
{
    public static class DTO
    {

        [Serializable]
        public class ObjectCreationRequest
        {
            public bool movable;
            public float[] vertices;
            public int[] triangles;
            public float position_x;
            public float position_y;
            public float position_z;
            public float rotation_w;
            public float rotation_x;
            public float rotation_y;
            public float rotation_z;
        }

        [Serializable]
        public class ObjectCreationResult
        {
            public bool movable;
            public int id;
        }

        [Serializable]
        public class ObjectsTrashRequest
        {
            public bool[] movables;
            public int[] ids;
        }

        [Serializable]
        public class CollisionRequest
        {
            public int movable_id;
            public float position_x;
            public float position_y;
            public float position_z;
            public float rotation_w;
            public float rotation_x;
            public float rotation_y;
            public float rotation_z;
        };

        [Serializable]
        public class MultipleCollisionRequest
        {
            public int movable_id;
            public float[] position_x;
            public float[] position_y;
            public float[] position_z;
            public float[] rotation_w;
            public float[] rotation_x;
            public float[] rotation_y;
            public float[] rotation_z;
        };

        [Serializable]
        public class NewLocalClosestResult
        {
            public bool successful;
            public float position_x;
            public float position_y;
            public float position_z;
            public float rotation_w;
            public float rotation_x;
            public float rotation_y;
            public float rotation_z;
        };

        [Serializable]
        public class NewLocalClosestRequest
        {
            public int movable_id;

            public float start_position_x;
            public float start_position_y;
            public float start_position_z;
            public float start_rotation_w;
            public float start_rotation_x;
            public float start_rotation_y;
            public float start_rotation_z;

            public float end_position_x;
            public float end_position_y;
            public float end_position_z;
            public float end_rotation_w;
            public float end_rotation_x;
            public float end_rotation_y;
            public float end_rotation_z;
        };

        [Serializable]
        public class PathToRequest
        {
            public int movable_id;

            // start positions
            public float start_position_x;
            public float start_position_y;
            public float start_position_z;
            public float start_rotation_w;
            public float start_rotation_x;
            public float start_rotation_y;
            public float start_rotation_z;

            // u path
            public float[] u_positions_x;
            public float[] u_positions_y;
            public float[] u_positions_z;
            public float[] u_rotations_w;
            public float[] u_rotations_x;
            public float[] u_rotations_y;
            public float[] u_rotations_z;
        };

        [Serializable]
        public class PathToResult
        {
            public int path_to_request_id;
            public bool successful;
        };

        [Serializable]
        public class PathToStatusRequest
        {
            public int path_to_request_id;
        };

        public class PathToStatusResult
        {
            public int path_to_request_id;
            public bool finished;
        };

        [Serializable]
        public class PathToGetRequest
        {
            public int path_to_request_id;
            public int movable_id;
        };

        public class PathToGetResult
        {
            public int matchee_result;
            // p path
            public float[] p_positions_x;
            public float[] p_positions_y;
            public float[] p_positions_z;
            public float[] p_rotations_w;
            public float[] p_rotations_x;
            public float[] p_rotations_y;
            public float[] p_rotations_z;
        };


        [Serializable]
        public class CollisionResult
        {
            public bool is_colliding;
        };

        [Serializable]
        public class MovedRequest
        {            
            public int movable_id;

            public float start_position_x;
            public float start_position_y;
            public float start_position_z;
            public float start_rotation_w;
            public float start_rotation_x;
            public float start_rotation_y;
            public float start_rotation_z;

            public float end_position_x;
            public float end_position_y;
            public float end_position_z;
            public float end_rotation_w;
            public float end_rotation_x;
            public float end_rotation_y;
            public float end_rotation_z;
        };
    }
}
