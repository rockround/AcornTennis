using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticInfoContainer
{
    //Used on stage load to determine which bot to spawn
    internal static int currentDifficulty;

    //Used on stage completion, along with difficulty to determine progression. 0-2, 0: Urban, 1:Suburban, 2:Rural
    internal static int currentStage;

    //Track of current progress through stages, used on load and on stage completion. This has the most recent stage defeated
    internal static int difficultyProgressUrban;
    internal static int difficultyProgressSuburban;
    internal static int difficultyProgressRural;


    //Settings
    internal static bool useDiscrete;
    internal static bool hideMouseDefault;

    internal static bool showTutorial;
}
