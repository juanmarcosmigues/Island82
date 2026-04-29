using UnityEngine;

public static class GameDefinitions
{
    public const int EXECUTION_ORDER_MOVING_SURFACE = -2;
    public const int EXECUTION_ORDER_PLAYER = -1;
    public const int EXECUTION_ORDER_PHYSICS_CALCULATIONS = 1;

    public const string SCENE_GAMEOVER = "SceneGameOver";

    public const string LAYER_STATICSOLID = "StaticSolid";
    public const string LAYER_DYNAMICSOLID = "DynamicSolid";
    public const string LAYER_COLLISIONONLY = "CollisionOnly";
    public const string LAYER_PLAYER = "Player";
    public const string LAYER_TRIGGER = "Trigger";

    public const int UI_CHARACTERS_PER_SECOND = 30;

    public const string GAME_ASSET_MATERIAL_DEFAULT = "Material_Default";
    public const string GAME_ASSET_MATERIAL_DARKROOM = "Material_DarkRoom";

    public static readonly Color[] Colors = new Color[]
    {
    // Darkest ? brightest
        new Color32(  5,  26,  19, 255), //  0  (was 12)
        new Color32( 14,  34,  25, 255), //  1  (was 13)
        new Color32( 19,  45,  32, 255), //  2  (was 14)
        new Color32( 28,  61,  42, 255), //  3  (was 15)
        new Color32( 37,  72,  42, 255), //  4  (was  8)
        new Color32( 42,  84,  48, 255), //  5  (was  9)
        new Color32( 60, 105,  50, 255), //  6  (was 10)
        new Color32( 82, 126,  47, 255), //  7  (was 11)
        new Color32(123, 162,  45, 255), //  8  (was  4)
        new Color32(149, 184,  32, 255), //  9  (was  5)
        new Color32(165, 202,  47, 255), // 10  (was  6)
        new Color32(181, 214,  61, 255), // 11  (was  7)
        new Color32(195, 222,  67, 255), // 12  (was  0)
        new Color32(212, 240,  80, 255), // 13  (was  1)
        new Color32(230, 252, 105, 255), // 14  (was  2)
        new Color32(252, 255, 142, 255), // 15  (was  3)
    };
    public static Color PickColor(int index) => Colors[index];
}
