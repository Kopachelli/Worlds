﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class ClanTribeSplitDecision : PolityDecision {

	public const float BaseMinPreferencePercentChange = 0.15f;
	public const float BaseMaxPreferencePercentChange = 0.30f;

	public const float BaseMinRelationshipPercentChange = 0.05f;
	public const float BaseMaxRelationshipPercentChange = 0.15f;

	public const float BaseMinProminencePercentChange = 0.05f;
	public const float BaseMaxProminencePercentChange = 0.15f;

	private Tribe _tribe;

	private bool _cantPrevent = false;
	private bool _preferSplit = true;

	private float _tribeChanceOfSplitting;

	private Clan _splitClan;
	private Clan _dominantClan;

	private static string GenerateDescriptionIntro (Tribe tribe, Clan splitClan) {

		return 
			"The pressures of distance and strained relationships has made most of the populance under clan " + splitClan.Name.BoldText + " to feel that " +
			"they are no longer part of the " + tribe.Name.BoldText + " tribe and wish for the clan to become their own tribe.\n\n";
	}

	public ClanTribeSplitDecision (Tribe tribe, Clan splitClan, Clan dominantClan, float tribeChanceOfSplitting) : base (tribe) {

		_tribe = tribe;

		Description = GenerateDescriptionIntro (tribe, splitClan) +
			"Unfortunately, the pressure is too high for the clan leader, " + splitClan.CurrentLeader.Name.BoldText + ", to do anything other than to acquiesce " +
			"to the demands of " + splitClan.CurrentLeader.PossessiveNoun + " people...";

		_cantPrevent = true;

		_tribeChanceOfSplitting = tribeChanceOfSplitting;

		_splitClan = splitClan;
		_dominantClan = dominantClan;
	}

	public ClanTribeSplitDecision (Tribe tribe, Clan splitClan, Clan dominantClan, bool preferSplit, float tribeChanceOfSplitting) : base (tribe) {

		_tribe = tribe;

		Description = GenerateDescriptionIntro (tribe, splitClan) +
			"Should the clan leader, " + splitClan.CurrentLeader.Name.BoldText + ", follow the wish of " + splitClan.CurrentLeader.PossessiveNoun + " people " +
			"and try to create a tribe of their own?";

		_preferSplit = preferSplit;

		_tribeChanceOfSplitting = tribeChanceOfSplitting;

		_splitClan = splitClan;
		_dominantClan = dominantClan;
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

		_dominantClan = _tribe.DominantFaction as Clan;

		float oldDominantProminenceValue = _dominantClan.Prominence;

		float minValChangeDominant = oldDominantProminenceValue + oldProminenceValue - minValChange;
		float maxValChangeDominant = oldDominantProminenceValue + oldProminenceValue - maxValChange;

		effectSplitClan = "Clan " + _splitClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" tribe (" + oldProminenceValue.ToString ("P") + ") decreases to: " + minValChange.ToString ("P") + " - " + maxValChange.ToString ("P");

		effectDominantClan = "Clan " + _dominantClan.Name.BoldText + ": prominence within the " + _tribe.Name.BoldText + 
			" tribe (" + oldDominantProminenceValue.ToString ("P") + ") increases to: " + minValChangeDominant.ToString ("P") + " - " + maxValChangeDominant.ToString ("P");
	}

	private string GeneratePreventSplitResultEffectsString_Relationship () {

		_dominantClan = _tribe.DominantFaction as Clan;

		float charismaFactor = _splitClan.CurrentLeader.Charisma / 10f;
		float wisdomFactor = _splitClan.CurrentLeader.Wisdom / 15f;

		float attributesFactor = Mathf.Max (charismaFactor, wisdomFactor);
		attributesFactor = Mathf.Clamp (attributesFactor, 0.5f, 2f);

		float minPercentChange = BaseMinRelationshipPercentChange * attributesFactor;
		float maxPercentChange = BaseMaxRelationshipPercentChange * attributesFactor;

		float originalValue = _splitClan.GetRelationshipValue (_dominantClan);

		float minValChange = MathUtility.IncreaseByPercent (originalValue, minPercentChange);
		float maxValChange = MathUtility.IncreaseByPercent (originalValue, maxPercentChange);

		return "Clan " + _splitClan.Name.BoldText + ": relationship with clan " + _dominantClan.Name.BoldText + " (" + originalValue.ToString ("0.00") + ") increases to: " + 
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

	public static void LeaderPreventsSplit (Clan splitClan, Clan dominantClan, Tribe tribe) {

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

		splitClan.SetToUpdate ();
		dominantClan.SetToUpdate ();

		tribe.AddEventMessage (new SplitClanPreventTribeSplitEventMessage (splitClan, tribe, splitClan.CurrentLeader, splitClan.World.CurrentDate));
	}

	private void PreventSplit () {

		LeaderPreventsSplit (_splitClan, _dominantClan, _tribe);
	}

	private string GenerateAllowSplitResultMessage () {

		string message = "\t• Clan " + _splitClan.Name.BoldText + " will attempt to leave the " + _tribe.Name.BoldText + " tribe and form a tribe of their own";

		return message;
	}

	public static void LeaderAllowsSplit (Clan splitClan, Clan dominantClan, Tribe originalTribe, float tribeChanceOfSplitting) {

		World world = originalTribe.World;

		bool tribePreferSplit = originalTribe.GetNextLocalRandomFloat (RngOffsets.TRIBE_SPLITTING_EVENT_TRIBE_PREFER_SPLIT) < tribeChanceOfSplitting;

		if (originalTribe.IsUnderPlayerFocus || dominantClan.IsUnderPlayerGuidance) {

			Decision tribeDecision;

			if (tribeChanceOfSplitting >= 1) {
				tribeDecision = new TribeSplitDecision (originalTribe, splitClan, dominantClan); // Player that controls dominant clan can't prevent splitting from happening
			} else {
				tribeDecision = new TribeSplitDecision (originalTribe, splitClan, dominantClan, tribePreferSplit); // Give player options
			}

			if (dominantClan.IsUnderPlayerGuidance) {

				world.AddDecisionToResolve (tribeDecision);

			} else {

				tribeDecision.ExecutePreferredOption ();
			}

		} else if (tribePreferSplit) {

			TribeSplitDecision.LeaderAllowsSplit (splitClan, dominantClan, originalTribe);

		} else {

			TribeSplitDecision.LeaderPreventsSplit (splitClan, dominantClan, originalTribe);
		}
	}

	private void AllowSplit () {

		LeaderAllowsSplit (_splitClan, _dominantClan, _tribe, _tribeChanceOfSplitting);
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
	