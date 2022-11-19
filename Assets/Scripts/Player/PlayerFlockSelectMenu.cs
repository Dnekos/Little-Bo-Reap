using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFlockSelectMenu : MonoBehaviour
{
    public SheepTypes flockToChangeTo;

    public void SetFlockBuild()
    {
        flockToChangeTo = SheepTypes.BUILD;
    }
    public void SetFlockRam()
    {
        flockToChangeTo = SheepTypes.RAM;
    }
    public void SetFlockFluffy()
    {
        flockToChangeTo = SheepTypes.FLUFFY;
    }
}
