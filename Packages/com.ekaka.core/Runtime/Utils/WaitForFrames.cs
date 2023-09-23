using UnityEngine;

namespace Core.Utils
{
    public class WaitForFrames : CustomYieldInstruction
    {
        private readonly int _targetFrameCount;
 
        public WaitForFrames(int numberOfFrames)
        {
            _targetFrameCount = Time.frameCount + numberOfFrames;
        }
 
        public override bool keepWaiting => Time.frameCount < _targetFrameCount;
    }
}