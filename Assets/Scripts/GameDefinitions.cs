using UnityEngine;

public static class GameDefinitions
{
    public const int EXECUTION_ORDER_MOVING_SURFACE = -2;
    public const int EXECUTION_ORDER_PLAYER = -1;
    public const int EXECUTION_ORDER_PHYSICS_CALCULATIONS = 1;

    public const string LAYER_STATICSOLID = "StaticSolid";
    public const string LAYER_DYNAMICSOLID = "DynamicSolid";
    public const string LAYER_COLLISIONONLY = "CollisionOnly";
    public const string LAYER_PLAYER = "Player";
    public const string LAYER_TRIGGER = "Trigger";

    public const int UI_CHARACTERS_PER_SECOND = 30;

    public const string GAME_ASSET_MATERIAL_DEFAULT = "Material_Default";
    public const string GAME_ASSET_MATERIAL_DARKROOM = "Material_DarkRoom";
}
