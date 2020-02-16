﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;
using System;

public class TestContext : Context
{
    public TestContext() : base("testContext")
    {
    }
}

public class TestBooleanEntityAttribute : BooleanEntityAttribute
{
    public const string TestId = "testBoolAttribute";

    private bool _value;

    public TestBooleanEntityAttribute(Entity entity, bool value)
        : base(TestId, entity, null)
    {
        _value = value;
    }

    public override bool Value => _value;
}

public class TestNumericFunctionEntityAttribute : NumericEntityAttribute
{
    public const string TestId = "testNumericFunctionAttribute";

    private IBooleanExpression _argument;

    public TestNumericFunctionEntityAttribute(Entity entity, IExpression[] arguments)
        : base(TestId, entity, null)
    {
        if ((arguments == null) || (arguments.Length < 1))
        {
            throw new System.ArgumentException("Number of arguments less than 1");
        }

        _argument = ExpressionBuilder.ValidateBooleanExpression(arguments[0]);
    }

    public override float Value => (_argument.Value) ? 10 : 2;
}

public class TestEntity : Entity
{
    public const string TestEntityAttributeId = "testEntityAttribute";

    private class InternalEntity : Entity
    {
        public const string TestId = "internalEntity";

        private TestBooleanEntityAttribute _boolAttribute;

        public InternalEntity() : base(TestId)
        {
            _boolAttribute = new TestBooleanEntityAttribute(this, true);
        }

        protected override object _reference => this;

        public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
        {
            switch (attributeId)
            {
                case TestBooleanEntityAttribute.TestId:
                    return _boolAttribute;
            }

            return null;
        }
    }

    private InternalEntity _internalEntity = new InternalEntity();

    private TestBooleanEntityAttribute _boolAttribute;

    private FixedEntityEntityAttribute _entityAttribute;

    protected override object _reference => this;

    public TestEntity() : base("testEntity")
    {
        _boolAttribute =
            new TestBooleanEntityAttribute(this, false);
        _entityAttribute =
            new FixedEntityEntityAttribute(_internalEntity, TestEntityAttributeId, this, null);
    }

    public override EntityAttribute GetAttribute(string attributeId, IExpression[] arguments = null)
    {
        switch (attributeId)
        {
            case TestBooleanEntityAttribute.TestId:
                return _boolAttribute;

            case TestEntityAttributeId:
                return _entityAttribute;

            case TestNumericFunctionEntityAttribute.TestId:
                return new TestNumericFunctionEntityAttribute(this, arguments);
        }

        return null;
    }
}

public class TestPolity : Polity
{
    public TestPolity(string type, CellGroup coreGroup) : base(type, coreGroup)
    {
    }

    public override float CalculateGroupProminenceExpansionValue(CellGroup sourceGroup, CellGroup targetGroup, float sourceValue)
    {
        throw new NotImplementedException();
    }

    public override void InitializeInternal()
    {
        throw new NotImplementedException();
    }

    protected override void GenerateEventsFromData()
    {
        throw new NotImplementedException();
    }

    protected override void GenerateName()
    {
        throw new NotImplementedException();
    }

    protected override void UpdateInternal()
    {
        throw new NotImplementedException();
    }
}

public class TestFaction : Faction
{
    private float _adminLoad;

    public TestFaction(
        string type, Polity polity, CellGroup coreGroup, float influence, float adminLoad)
        : base(type, polity, coreGroup, influence)
    {
        _adminLoad = adminLoad;

        Culture = new FactionCulture(this);

        Culture.AddPreference(new CulturalPreference(
            CulturalPreference.AuthorityPreferenceId,
            CulturalPreference.AuthorityPreferenceName,
            CulturalPreference.AuthorityPreferenceRngOffset,
            0));

        Culture.AddPreference(new CulturalPreference(
            CulturalPreference.CohesionPreferenceId,
            CulturalPreference.CohesionPreferenceName,
            CulturalPreference.CohesionPreferenceRngOffset,
            0));

        Culture.AddPreference(new CulturalPreference(
            CulturalPreference.IsolationPreferenceId,
            CulturalPreference.IsolationPreferenceName,
            CulturalPreference.IsolationPreferenceRngOffset,
            0));
    }

    public override void Split()
    {
        throw new NotImplementedException();
    }

    protected override float CalculateAdministrativeLoad()
    {
        return _adminLoad;
    }

    protected override void GenerateEventsFromData()
    {
        throw new NotImplementedException();
    }

    protected override void GenerateName(Faction parentFaction)
    {
    }

    protected override Agent RequestCurrentLeader()
    {
        throw new NotImplementedException();
    }

    protected override Agent RequestNewLeader()
    {
        throw new NotImplementedException();
    }

    protected override void UpdateInternal()
    {
        throw new NotImplementedException();
    }
}
