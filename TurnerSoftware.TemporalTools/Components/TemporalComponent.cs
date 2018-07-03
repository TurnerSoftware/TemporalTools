using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TurnerSoftware.TemporalTools.Components
{
	public abstract class TemporalComponent
	{
		public string Text { get; private set; }
		public int StartPosition { get; private set; }
		public int EndPosition { get; private set; }

		internal TemporalComponent(Match match)
		{
			Text = match.Value;
			StartPosition = match.Index;
			EndPosition = match.Index + match.Length;
		}

		/// <summary>
		/// Groups components based on proximity
		/// </summary>
		/// <param name="components"></param>
		/// <returns></returns>
		internal static IEnumerable<IEnumerable<TemporalComponent>> GroupComponents(IEnumerable<TemporalComponent> components)
		{
			var orderedComponents = components.OrderBy(c => c.StartPosition).ToList();
			var result = new List<IEnumerable<TemporalComponent>>();

			for (int i = 0, l = orderedComponents.Count; i < l; i++)
			{
				var currentComponent = orderedComponents[i];
				var componentGroup = new List<TemporalComponent> { currentComponent };
				
				while (i + 1 < l)
				{
					var nextComponent = orderedComponents[i + 1];
					var nextComponentDistance = nextComponent.StartPosition - currentComponent.EndPosition;

					if (nextComponentDistance > 1)
					{
						//The next component is too far away from the current
						//eg. [Thursday] is good [at 9:30am]
						break;
					}
					else if (nextComponentDistance == 1)
					{
						//The next component follows on from the current
						//eg. [Thursday] [at 9:30am]
						componentGroup.Add(nextComponent);
						currentComponent = nextComponent;
						i++;
					}
					else
					{
						//The next component begins inside the current
						if (nextComponent.Text.Length > currentComponent.Text.Length)
						{
							//If the next component is longer than the current, we replace the current with it
							//ie. longer == more specific
							componentGroup[componentGroup.Count - 1] = nextComponent;
							currentComponent = nextComponent;
						}
						i++;
					}
				}

				result.Add(componentGroup);
			}

			return result;
		}

		public abstract ITimePeriod GetTimePeriod(DateTime baseDate);
	}
}
