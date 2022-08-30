namespace IMP
{
    public class IMPStaticObject : IMPObject
    {
        public void Start()
        {
            BaseStart();
        }

        public void Update()
        {
            CreateRemote(false);
            BaseUpdate();
        }
    }
}