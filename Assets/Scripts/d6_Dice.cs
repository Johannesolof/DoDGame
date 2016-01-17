using UnityEngine;
using System.Collections.Generic;

public class d6_Dice : BaseDice {

    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 1, Direction = new Vector3(0, 1, 0)},
            new Normal {Value = 2, Direction = new Vector3(0, 0, -1)},
            new Normal {Value = 3, Direction = new Vector3(1, 0, 0)},
            new Normal {Value = 4, Direction = new Vector3(-1, 0, 0)},
            new Normal {Value = 5, Direction = new Vector3(0, 0, 1)},
            new Normal {Value = 6, Direction = new Vector3(0, -1, 0)},
        };
    }
}
