using UnityEngine;

class LerpAnimation
{
    private float _alpha = 0.0f;
    private IMP.Configuration _start, _end;

    public float Increment { get; set; }

    public LerpAnimation(IMP.Configuration start, IMP.Configuration end)
    {
        Increment = 0.05f;
        _start = start;
        _end = end;
    }

    public IMP.Configuration Update(GameObject obj)
    {
        _alpha += Increment;
        if (_alpha > 1.0f)
            _alpha = 1.0f;

        return new IMP.Configuration{
            Position = Vector3.Lerp(_start.Position, _end.Position, _alpha),
            Rotation = Quaternion.Lerp(_start.Rotation, _end.Rotation, _alpha)
        };
    }
}