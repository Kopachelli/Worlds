﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public abstract class NumericEntityAttribute : EntityAttribute
{
    public NumericEntityAttribute(string id, Entity entity, IExpression[] arguments)
        : base(id, entity, arguments)
    { }

    public abstract float Value { get; }

    protected override EntityAttributeExpression BuildExpression()
    {
        return new NumericEntityAttributeExpression(this);
    }
}