﻿using System;

public class ConditionSetPropertyEntity : PropertyEntity
{
    public const string ValueId = "value";

    private bool _value;

    public IBooleanExpression[] Conditions;

    private EntityAttribute _valueAttribute;

    private class ValueAttribute : BooleanEntityAttribute
    {
        private ConditionSetPropertyEntity _propertyEntity;

        public ValueAttribute(ConditionSetPropertyEntity propertyEntity)
            : base(ValueId, propertyEntity, null)
        {
            _propertyEntity = propertyEntity;
        }

        public override bool Value => _propertyEntity.GetValue();
    }

    public ConditionSetPropertyEntity(Context context, Context.LoadedProperty p)
        : base(context, p)
    {
        if (p.conditions == null)
        {
            throw new ArgumentException("'conditions' list can't be empty");
        }

        Conditions = ExpressionBuilder.BuildBooleanExpressions(context, p.conditions);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case ValueId:
                _valueAttribute =
                    _valueAttribute ??
                    new ValueAttribute(this);
                return _valueAttribute;
        }

        throw new System.ArgumentException(Id + " property: Unable to find attribute: " + attributeId);
    }

    public bool GetValue()
    {
        EvaluateIfNeeded();

        return _value;
    }

    protected override void Calculate()
    {
        _value = true;

        foreach (IBooleanExpression exp in Conditions)
        {
            _value &= exp.Value;

            if (!_value)
                break;
        }
    }
}