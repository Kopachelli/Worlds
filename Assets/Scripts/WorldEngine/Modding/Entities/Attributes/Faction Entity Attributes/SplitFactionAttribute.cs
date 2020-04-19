﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SplitFactionAttribute : EffectEntityAttribute
{
    private FactionEntity _factionEntity;

    private ModDecision _decisionToTrigger = null;

    private readonly IValueExpression<Entity> _coreGroupArg;
    private readonly IValueExpression<float> _influenceTransferArg;
    private readonly IValueExpression<float> _relationshipValueArg;
    private readonly IValueExpression<string> _newFactionTypeArg = null;

    public SplitFactionAttribute(FactionEntity factionEntity, IExpression[] arguments)
        : base(FactionEntity.SplitFactionAttributeId, factionEntity, arguments)
    {
        _factionEntity = factionEntity;

        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException("Number of arguments less than 3");
        }

        _coreGroupArg =
            ValueExpressionBuilder.ValidateValueExpression<Entity>(arguments[0]);
        _influenceTransferArg =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[1]);
        _relationshipValueArg =
            ValueExpressionBuilder.ValidateValueExpression<float>(arguments[2]);

        if (arguments.Length > 3)
        {
            _newFactionTypeArg =
                ValueExpressionBuilder.ValidateValueExpression<string>(arguments[3]);
        }
    }

    public override void Apply()
    {
        if (!(_coreGroupArg.Value is GroupEntity groupEntity))
        {
            throw new System.Exception(
                "Seconds argument is not a group entity: " + _coreGroupArg.Value.GetType());
        }

        Faction faction = _factionEntity.Faction;

        string newFactionType;
        if (_newFactionTypeArg != null)
        {
            newFactionType = _newFactionTypeArg.Value;
        }
        else
        {
            newFactionType = faction.Type;
        }

        faction.Split(
            newFactionType,
            groupEntity.Group,
            _influenceTransferArg.Value,
            _relationshipValueArg.Value);
    }
}