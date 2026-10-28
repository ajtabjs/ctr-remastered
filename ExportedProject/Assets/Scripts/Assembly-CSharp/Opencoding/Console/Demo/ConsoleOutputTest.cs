using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Opencoding.Console.Demo
{
	public class ConsoleOutputTest : MonoBehaviour
	{
		private static string[] _randomStartupText = new string[]
		{
			"Adding Hidden Agendas",
			"Adjusting Bell Curves",
			"Aesthesizing Industrial Areas",
			"Aligning Covariance Matrices",
			"AnyKey",
			"AnyKey",
			"Asserting Packed Exemplars",
			"Attempting to Lock Back-Buffer",
			"Binding Sapling Root System",
			"Breeding Fauna",
			"Building Data Trees",
			"Bureacritizing Bureaucracies",
			"Calculating Inverse Probability Matrices",
			"Calculating Llama Expectoration Trajectory",
			"Calibrating Blue Skies",
			"Charging Ozone Layer",
			"Coalescing Cloud Formations",
			"Cohorting Exemplars",
			"Collecting Meteor Particles",
			"Compounding Inert Tessellations",
			"Compressing Fish Files",
			"Computing Optimal Bin Packing",
			"Concatenating Sub-Contractors",
			"Containing Existential Buffer",
			"Debarking Ark Ramp",
			"Debunching Unionized Commercial Services",
			"Deciding What Message to Display Next",
			"Decomposing Singular Values",
			"Decrementing Tectonic Plates",
			"Deleting Ferry Routes",
			"Depixelating Inner Mountain Surface Back Faces",
			"Depositing Slush Funds",
			"Destabilizing Economic Indicators",
			"Determining Width of Blast Fronts",
			"Deunionizing Bulldozers",
			"Dicing Models",
			"Digital",
			"Double",
			"Exposing Flash Variables to Streak System",
			"Extracting Resources",
			"Factoring Pay Scale",
			"Fixing Election Outcome Matrix",
			"Flood-Filling Ground Water",
			"Flushing Pipe Network",
			"Gamepad",
			"Generating Jobs",
			"Gesticulating Mimes",
			"Graphing Whale Migration",
			"Hiding Willio Webnet Mask",
			"Implementing Impeachment Routine",
			"Increasing Accuracy of RCI Simulators",
			"Increasing Magmafacation",
			"Initializing My Sim Tracking Mechanism",
			"Initializing Rhinoceros Breeding Timetable",
			"Initializing Robotic Click-Path AI",
			"Inserting Sublimated Messages",
			"Integer",
			"Integer",
			"Integer",
			"Iterating Cellular Automata",
			"Lecturing Errant Subsystems",
			"Mixing Genetic Pool",
			"Modeling Object Components",
			"Mopping Occupant Leaks",
			"Normalizing Power",
			"Obfuscating Quigley Matrix",
			"Overconstraining Dirty Industry Calculations",
			"Partitioning City Grid Singularities",
			"Pen",
			"Pixalating Nude Patch",
			"Pointer",
			"Pointer",
			"Preparing Sprites for Random Walks",
			"Prioritizing Landmarks",
			"Projecting Law Enforcement Pastry Intake",
			"Realigning Alternate Time Frames",
			"Reconfiguring User Mental Processes",
			"Relaxing Splines",
			"RemoteActivationService.rem",
			"RemoteActivationService.rem",
			"RemoteActivationService.rem",
			"Requested Vertex count is above the limit. Alloc will fail.",
			"Reticulating Splines",
			"Retracting Phong Shader",
			"Retrieving from Back Store",
			"Reverse Engineering Image Consultant",
			"Routing Neural Network Infanstructure",
			"Saving in-memory dirty slot {0} to disk.",
			"Scrubbing Terrain",
			"Searching for Llamas",
			"Seeding Architecture Simulation Parameters",
			"Sensor",
			"Setting Advisor Moods",
			"Setting Inner Deity Indicators",
			"Setting Universal Physical Constants",
			"Sonically Enhancing Occupant-Free Timber",
			"Speculating Stock Market Indices",
			"Splatting Transforms",
			"StepCounter",
			"Sub-Sampling Water Data",
			"Synthesizing Gravity",
			"Synthesizing Wavelets",
			"Time-Compressing Simulator Clock",
			"Unable to Reveal Current Activity",
			"Weathering Buildings",
			"Zeroing Crime Network"
		};

		private void Start()
		{
			PerformFakeStartupFlow();
			CommandHandlers.RegisterCommandHandlers(this);
		}

		private static void PerformFakeStartupFlow()
		{
			for (int i = 10; i != 0; i--)
			{
				Debug.Log(_randomStartupText[Random.Range(0, _randomStartupText.Length)]);
				int num = Random.Range(0, 5);
				if (num == 4)
				{
					Debug.LogWarning("Something went wrong");
				}
				else if (num == 3)
				{
					Debug.LogError("Failed");
				}
			}
			Debug.Log("Startup is complete!");
		}
	}
}
