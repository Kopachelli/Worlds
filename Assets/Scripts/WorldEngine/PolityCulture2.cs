using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.Profiling;

public class PolityCulture2 : Culture {

	[XmlIgnore]
	public Polity Polity;

	[XmlIgnore]
	private float _totalGroupInfluenceValue;

	public PolityCulture2 () {
	
	}

	public PolityCulture2 (Polity polity) : base (polity.World) {

		Polity = polity;

		#if DEBUG
		if (World.SelectedCell != null && 
			World.SelectedCell.Group != null) {

			if (World.SelectedCell.Group.GetPolityInfluenceValue (Polity) > 0) {

				Debug.Log ("Debug Selected");
			}
		}
		#endif

		CellGroup coreGroup = Polity.CoreGroup;

		if (coreGroup == null)
			throw new System.Exception ("CoreGroup can't be null at this point");

		CellCulture coreCulture = coreGroup.Culture;

		foreach (CulturalPreference p in coreCulture.Preferences) {
			AddPreference (new CulturalPreference (p));
		}

		foreach (CulturalActivity a in coreCulture.Activities) {
			AddActivity (new CulturalActivity (a));
		}

		foreach (CulturalSkill s in coreCulture.Skills) {
			AddSkill (new CulturalSkill (s));
		}

		foreach (CulturalKnowledge k in coreCulture.Knowledges) {
			PolityCulturalKnowledge knowledge = new PolityCulturalKnowledge (k);
			AddKnowledge (knowledge);
		}

		foreach (CulturalDiscovery d in coreCulture.Discoveries) {
			PolityCulturalDiscovery discovery = new PolityCulturalDiscovery (d);
			AddDiscovery (discovery);
			discovery.PresenceCount++;
		}

		Language = coreCulture.Language;

		if (Language == null) {
		
			GenerateNewLanguage ();
		}
	}

	public float GetNextRandomFloat (int rngOffset) {

		return Polity.GetNextLocalRandomFloat (rngOffset);
	}

	private void GenerateNewLanguage () {

		Language = new Language (Polity.GenerateUniqueIdentifier (World.CurrentDate, 100L, Polity.Id));

		// Generate Articles

		Language.GenerateArticleProperties ();

		Language.GenerateArticleAdjunctionProperties ();
		Language.GenerateArticleSyllables ();
		Language.GenerateAllArticles ();

		// Generate Noun Indicatives

		Language.GenerateNounIndicativeProperties ();

		Language.GenerateNounIndicativeAdjunctionProperties ();
		Language.GenerateNounIndicativeSyllables ();
		Language.GenerateAllNounIndicatives ();

		// Generate Verb Indicatives

		Language.GenerateVerbIndicativeProperties ();

		Language.GenerateVerbIndicativeAdjunctionProperties ();
		Language.GenerateVerbIndicativeSyllables ();
		Language.GenerateAllVerbIndicatives ();

		// Generate Noun, Adjective and Adposition Properties and Syllables

		Language.GenerateVerbSyllables ();

		Language.GenerateNounAdjunctionProperties ();
		Language.GenerateNounSyllables ();

		Language.GenerateAdjectiveAdjunctionProperties ();
		Language.GenerateAdjectiveSyllables ();

		Language.GenerateAdpositionAdjunctionProperties ();
		Language.GenerateAdpositionSyllables ();

		World.AddLanguage (Language);
	}

	public void Update () {

		ResetAttributeValues ();

		AddGroupCultures ();

		NormalizeAttributeValues ();
	}

	private void ResetAttributeValues () {

		foreach (CulturalPreference preference in Preferences) {
			preference.Value = 0;
		}

		foreach (CulturalActivity activity in Activities) {
			activity.Value = 0;
			activity.Contribution = 0;
		}

		foreach (CulturalSkill skill in Skills) {
			skill.Value = 0;
		}

		foreach (PolityCulturalKnowledge knowledge in Knowledges) {
			knowledge.AggregateValue = 0;
			knowledge.Value = 0;
		}

		foreach (PolityCulturalDiscovery discovery in Discoveries) {
			discovery.PresenceCount = 0;
		}
	}

	private void NormalizeAttributeValues () {

//		#if DEBUG
//		if (Manager.RegisterDebugEvent != null) {
//			Manager.RegisterDebugEvent ("DebugMessage", 
//				"NormalizeAttributeValues - Polity:" + Polity.Id + 
//				", CurrentDate: " + World.CurrentDate + 
//				", Activities.Count: " + Activities.Count + 
//				", Skills.Count: " + Skills.Count + 
//				", Knowledges.Count: " + Knowledges.Count + 
//				", Polity.TotalGroupInfluenceValue: " + Polity.TotalGroupInfluenceValue + 
//				"");
//		}
//		#endif

		if (Polity.InfluencedGroups.Count <= 0)
			return;

		foreach (CulturalPreference preference in Preferences) {

			preference.Value = MathUtility.RoundToSixDecimals(Mathf.Clamp01 (preference.Value/_totalGroupInfluenceValue));
		}

		foreach (CulturalActivity activity in Activities) {

			activity.Value = MathUtility.RoundToSixDecimals(Mathf.Clamp01 (activity.Value/_totalGroupInfluenceValue));
			activity.Contribution = MathUtility.RoundToSixDecimals(Mathf.Clamp01 (activity.Contribution/_totalGroupInfluenceValue));
		}

		foreach (CulturalSkill skill in Skills) {

			float realValue = skill.Value / _totalGroupInfluenceValue;

			#if DEBUG
			if ((realValue > 1.1f) || (realValue < -0.1f)) {
				throw new System.Exception ("Polity Skill value way out of bounds (-0.1f,1.1f): " + realValue);
			}
			#endif

			skill.Value = MathUtility.RoundToSixDecimals(Mathf.Clamp01(realValue));
		}

		foreach (PolityCulturalKnowledge knowledge in Knowledges) {


			float d;
			int newValue = (int)MathUtility.DivideAndGetDecimals (knowledge.AggregateValue, _totalGroupInfluenceValue, out d);

			if (d > GetNextRandomFloat (RngOffsets.POLITY_CULTURE_NORMALIZE_ATTRIBUTE_VALUES))
				newValue++;

			knowledge.Value = newValue;
		}
	}

	private void AddGroupCultures () {

		_totalGroupInfluenceValue = 0;

		foreach (CellGroup group in Polity.InfluencedGroups.Values) {
		
			AddGroupCulture (group);
		}
	}

	private void AddGroupCulture (CellGroup group) {

//		#if DEBUG
//		if (World.SelectedCell != null && 
//			World.SelectedCell.Group != null) {
//
//			if (World.SelectedCell.Group.GetPolityInfluenceValue (Polity) > 0) {
//
//				Debug.Log ("Debug Selected");
//			}
//		}
//		#endif

		float influenceValue = group.GetPolityInfluenceValue (Polity);

		_totalGroupInfluenceValue += influenceValue;

		if (influenceValue <= 0) {

			throw new System.Exception ("Polity [" + Polity.Id + "] has influence value of " + influenceValue + " in Group [" + group.Id + "]. Current Date: " + World.CurrentDate);
		}

		foreach (CulturalPreference groupPreference in group.Culture.Preferences) {

			CulturalPreference preference = GetPreference (groupPreference.Id);

			if (preference == null) {

				preference = new CulturalPreference (groupPreference);
				preference.Value *= influenceValue;

				AddPreference (preference);

			} else {

				preference.Value += groupPreference.Value * influenceValue;
			}
		}

		foreach (CulturalActivity groupActivity in group.Culture.Activities) {
		
			CulturalActivity activity = GetActivity (groupActivity.Id);

			if (activity == null) {
			
				activity = new CulturalActivity (groupActivity);
				activity.Value *= influenceValue;
				activity.Contribution *= influenceValue;

				AddActivity (activity);

			} else {
			
				activity.Value += groupActivity.Value * influenceValue;
				activity.Contribution += groupActivity.Contribution * influenceValue;
			}
		}

		foreach (CulturalSkill groupSkill in group.Culture.Skills) {

			CulturalSkill skill = GetSkill (groupSkill.Id);

			if (skill == null) {

				skill = new CulturalSkill (groupSkill);
				skill.Value *= influenceValue;

				AddSkill (skill);

			} else {

				skill.Value += groupSkill.Value * influenceValue;
			}
		}

		foreach (CulturalKnowledge groupKnowledge in group.Culture.Knowledges) {

			PolityCulturalKnowledge knowledge = GetKnowledge (groupKnowledge.Id) as PolityCulturalKnowledge;

			if (knowledge == null) {

				knowledge = new PolityCulturalKnowledge (groupKnowledge);
				knowledge.AggregateValue = groupKnowledge.Value * influenceValue;

				AddKnowledge (knowledge);

			} else {
				
				knowledge.AggregateValue += groupKnowledge.Value * influenceValue;
			}
		}

		foreach (CulturalDiscovery groupDiscovery in group.Culture.Discoveries) {

			PolityCulturalDiscovery discovery = GetDiscovery (groupDiscovery.Id) as PolityCulturalDiscovery;

			if (discovery == null) {

				discovery = new PolityCulturalDiscovery (groupDiscovery);

				AddDiscovery (discovery);
			}

			discovery.PresenceCount++;
		}
	}
}