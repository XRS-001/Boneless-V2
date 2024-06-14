using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnumDeclaration : MonoBehaviour
{
    //A dedicated script for declaring enums to prevent hot reload issues
    public enum handTypeEnum { Left, Right }
    public enum upDirection { forward, up, right }
    public enum enemyTypeEnum { aggresive, passive, dummy }
    public enum surfaceType
    {
        Plaster,
        Metal,
        Folliage,
        Rock,
        Wood,
        Brick,
        Concrete,
        Dirt,
        Glass,
        Water
    }
    public enum turnType { none, smooth, snap}
}
