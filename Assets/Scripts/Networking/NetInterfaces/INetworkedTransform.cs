using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkedTransform : INetworked
{
	protected Vector3 NetPrevPos { get; set; }
}
