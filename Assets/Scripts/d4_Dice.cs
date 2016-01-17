using System.Collections.Generic;
using UnityEngine;

public class d4_Dice : BaseDice 
{
    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 1, Direction = new Vector3(-0.5f, -0.3f, 0.8f)},
            new Normal {Value = 2, Direction = new Vector3(-0.5f, -0.3f, -0.8f)},
            new Normal {Value = 3, Direction = new Vector3(0.9f, -0.3f, 0f)},
            new Normal {Value = 4, Direction = new Vector3(0f, 1f, 0f)},
        };
    }
}