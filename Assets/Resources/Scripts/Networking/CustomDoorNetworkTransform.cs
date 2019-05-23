using UnityEngine;
//CLEANED
namespace Mirror
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkTransform")]
    [HelpURL("https://vis2k.github.io/Mirror/Components/NetworkTransform")]
    public class CustomDoorNetworkTransform : CustomDoorNetworkTransformBase
    {
        protected override Transform targetComponent => transform;
    }
}
