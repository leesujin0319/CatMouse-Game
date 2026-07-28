using System;
using CatMouse.Game.Player;
using UnityEngine;

namespace CatMouse.Game.Run
{
    [Serializable]
    public struct RunItemModifier
    {
        [SerializeField] private PlayerStatType _statType;
        [SerializeField] private PlayerStatModifierOperation _operation;
        [SerializeField] private float _value;

        public PlayerStatType StatType => _statType;
        public PlayerStatModifierOperation Operation => _operation;
        public float Value => _value;
    }
}
