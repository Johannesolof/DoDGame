using UnityEngine;
using System.Collections.Generic;

public class d4_Dice : BaseDice {

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

public class d8_Dice : BaseDice
{

    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 1, Direction = new Vector3(0, 1, 0)},
            new Normal {Value = 2, Direction = new Vector3(-0.8f, 0.6f, -1)},
            new Normal {Value = 3, Direction = new Vector3(1, 0, 0)},
            new Normal {Value = 4, Direction = new Vector3(-1, 0, 0)},
            new Normal {Value = 5, Direction = new Vector3(0, 0, 1)},
            new Normal {Value = 6, Direction = new Vector3(0, -1, 0)},

            new Normal {Value = 7, Direction = new Vector3(0.8f, -0.6f, -1)},
            new Normal {Value = 8, Direction = new Vector3(-0.8f, -0.6f, -1)},
        };
    }
}
