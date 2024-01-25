using UnityEngine;

  [System.Serializable]
    public class MyData : MonoBehaviour
    {
        public string id;
        public string domainId;
        public SerializablePose pose;
    }
    
    [System.Serializable]
    public class SerializablePose
    {
        public Vector3 position;
        public Quaternion rotation;

        public SerializablePose(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public Pose ToPose()
        {
            return new Pose(position, rotation);
        }

        public static SerializablePose FromPose(Pose pose)
        {
            return new SerializablePose(pose.position, pose.rotation);
        }
    }
