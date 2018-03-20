﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ClanSplitFromTribeDecision : PolityDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinProminencePercentChange = 0.05f;
	public const float BaseMaxProminencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _cantPrevent = false;
	private bool _preferSplit = true;

	private Clan _splitClan;

	private DecisionEffectDelegate _allowSplitTriggerTribeDecision;

	private static string GenerateDescriptionIntro (Tribe tribe, Clan splitClan) {

		return 
			"The pressures of distance and strained relationships has made most of the populance under Clan " + splitClan.Name.BoldText + " to feel that " +
			"they are no longer part of the " + tribe.Name.BoldText + " Tribe and wish for the clan to become their own tribe.\n";
	}

	public ClanSplitFromTribeDecision (Tribe tribe, Clan splitClan, DecisionEffectDelegate allowSplitTriggerTribeDecision) : base (tribe) {

		_tribe = tribe;

		Description = GenerateDescriptionIntro (tribe, splitClan) +
			"Unfortunately, the pressure is too high for the clan leader, " + splitClan.CurrentLeader.Name.BoldText + ", to do anything other than to acquiesce " +
			"to the demands of " + splitClan.CurrentLeader.PossessiveNoun + " people...";

		_cantPrevent = true;

		_splitClan = splitClan;

		_allowSplitTriggerTribeDecision = allowSplitTriggerTribeDecision;
	}

	public ClanSplitFromTribeDecision (Tribe tribe, Clan splitClan, bool preferSplit, DecisionEffectDelegate allowSplitTriggerTribeDecision) : base (tribe) {

		_tribe = tribe;

		Description = GenerateDescriptionIntro (tribe, splitClan) +
			"Should the clan leader, " + splitClan.CurrentLeader.Name.BoldText + ", follow the wish of " + splitClan.CurrentLeader.PossessiveNoun + " people " +
			"and try to create a tribe of their own?";

		_preferSplit = preferSplit;

		_splitClan = splitClan;

		_allowSplitTriggerTribeDecision = allowSplitTriggerTribeDecision;
	}

	private string GeneratePreventSplitResultEffectsString_AuthorityPreference () {

		float charismaFactor = _splitClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _splitClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinPreferencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxPreferencePercentChange / attributesFactor;

		float originalValue = _splitClan.GetPreferenceValue (CulturalPreference.AuthorityPreferenceId);

		float minValChange = MathUtility.DecreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.DecreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _splitClan.Name.BoldText + ": authority preference (" + originalValue.ToString ("0.00") + ") decreases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private void GeneratePreventSplitResultEffectsString_Prominence (out string effectSplitClan, out string effectDominantClan) {

		float charismaFactor = _splitClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _splitClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinProminencePercentChange / attributesFactor;
		float maxPercentChange = BaseMaxProminencePercentChange / attributesFactor;

		float oldProminenceValue = _splitClan.Prominence;

		float minValChange = oldProminenceValue * (1f - minPercentChange);
		float maxValChange = oldProminenceValue * (1f - maxPercentChange);

		Clan dominantClan = _tribe.DominantFaction as Clan;

		float oldDominantProminenceValue = dominantClan.Prominence;

		float minValChangeDominant = oldDominantProminenceValue + oldProminenceValue - minValChange;
		float maxValChangeDominant = oldDominantProminenceValue + oldProminenceValue - maxValChange;

		effectSplitClan = "Clan " + _splitClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" Tribe (" + oldProminenceValue.ToString ("0.00") + ") decreases to: " + minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");

		effectDominantClan = "Clan " + dominantClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" Tribe (" + oldDominantProminenceValue.ToString ("0.00") + ") increases to: " + minValChangeDominant.ToString ("0.00") + " - " + maxValChangeDominant.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString_Relationship () {

		Clan dominantClan = _tribe.DominantFaction as Clan;

		float charismaFactor = _splitClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _splitClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinRelationshipPercentChange * attributesFactor;
		float maxPercentChange = BaseMaxRelationshipPercentChange * attributesFactor;

		float originalValue = _splitClan.GetRelationshipValue (dominantClan);

		float minValChange = MathUtility.IncreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _splitClan.Name.BoldText + ": relationship with Clan " + dominantClan.Name.BoldText + " (" + originalValue.ToString ("0.00") + ") increases to: " + 
			minValChange.ToString ("0.00") + " - " + maxValChange.ToString ("0.00");
	}

	private string GeneratePreventSplitResultEffectsString () {

		string splitClanProminenceChangeEffect;
		string dominantClanProminenceChangeEffect;

		GeneratePreventSplitResultEffectsString_Prominence (out splitClanProminenceChangeEffect, out dominantClanProminenceChangeEffect);

		return 
			"\t• " + GeneratePreventSplitResultEffectsString_AuthorityPreference () + "\n" + 
			"\t• " + GeneratePreventSplitResultEffectsString_Relationship () + "\n" + 
			"\t• " + splitClanProminenceChangeEffect + "\n" + 
			"\t• " + dominantClanProminenceChangeEffect;
	}

	public static void LeaderPreventsSplit (Clan splitClan, Tribe tribe) {

		Clan dominantClan = tribe.DominantFaction as Clan;

		float charismaFactor = splitClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = splitClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		int rngOffset = RngOffsets.TRIBE_SPLITTING_EVENT_SPLITCLAN_LEADER_PREVENTS_MODIFY_ATTRIBUTE;

		// Authority preference

		float randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float authorityPreferencePercentChange = (BaseMaxPreferencePercentChange - BaseMinPreferencePercentChange) * randomFactor + BaseMinPreferencePercentChange;
		authorityPreferencePercentChange /= attributesFactor;

		splitClan.DecreasePreferenceValue (CulturalPreference.AuthorityPreferenceId, authorityPreferencePercentChange);

		// Prominence

		randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float prominencePercentChange = (BaseMaxProminencePercentChange - BaseMinProminencePercentChange) * randomFactor + BaseMinProminencePercentChange;
		prominencePercentChange /= attributesFactor;

		Polity.TransferProminence (splitClan, dominantClan, prominencePercentChange);

		// Relationship

		randomFactor = splitClan.GetNextLocalRandomFloat (rngOffset++);
		float relationshipPercentChange = (BaseMaxRelationshipPercentChange - BaseMinRelationshipPercentChange) * randomFactor + BaseMinRelationshipPercentChange;
		relationshipPercentChange *= attributesFactor;

		float newValue = MathUtility.IncreaseByPercent (splitClan.GetRelationshipValue (dominantClan), relationshipPercentChange);
		Faction.SetRelationship (splitClan, dominantClan, newValue);

		// Updates

		splitClan.World.AddFactionToUpdate (splitClan);
		splitClan.World.AddFactionToUpdate (dominantClan);

		splitClan.World.AddPolityToUpdate (tribe);

		tribe.AddEventMessage (new SplitClanPreventTribeSplitEventMessage (splitClan, tribe, splitClan.CurrentLeader, splitClan.World.CurrentDate));
	}

	private void PreventSplit () {

		LeaderPreventsSplit (_splitClan, _tribe);
	}

	private string GenerateAllowSplitResultMessage () {

		string message = "\t• Clan " + _splitClan.Name.BoldText + " will attempt to leave the " + _tribe.Name.BoldText + " Tribe and form a tribe of their own";

		return message;
	}

	public static void LeaderAllowsSplit (Clan splitClan, Tribe originalTribe, DecisionEffectDelegate allowSplitTriggerTribeDecision) {

		allowSplitTriggerTribeDecision ();
	}

	private void AllowSplit () {

		LeaderAllowsSplit (_splitClan, _tribe, _allowSplitTriggerTribeDecision);
	}

	public override Option[] GetOptions () {

		if (_cantPrevent) {

			return new Option[] {
				new Option ("Oh well...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			};
		}

		return new Option[] {
			new Option ("Allow clan to form a new tribe...", "Effects:\n" + GenerateAllowSplitResultMessage (), AllowSplit),
			new Option ("Prevent clan from leaving tribe...", "Effects:\n" + GeneratePreventSplitResultEffectsString (), PreventSplit)
		};
	}

	public override void ExecutePreferredOption ()
	{
		if (_preferSplit)
			AllowSplit ();
		else
			PreventSplit ();
	}
}
	