health-analyzer-window-entity-brain-health-text = Brain Activity:
health-analyzer-window-entity-blood-pressure-text = Blood Pressure:
health-analyzer-window-entity-heart-rate-text = Heart Rate:
health-analyzer-window-entity-heart-health-text = Heart Health:
health-analyzer-window-entity-lung-health-text = Lung Health:
health-analyzer-window-entity-spo2-text = {LOC($spo2)}:
health-analyzer-window-entity-etco2-text = {LOC($etco2)}:
health-analyzer-window-entity-respiratory-rate-text = Respiratory Rate:

health-analyzer-window-entity-brain-health-value = {$value}%
health-analyzer-window-entity-heart-health-value = {$value}%
health-analyzer-window-entity-lung-health-value = {$value}%
health-analyzer-window-entity-heart-rate-value = {$value}bpm
health-analyzer-window-entity-blood-pressure-value = {$systolic}/{$diastolic}
health-analyzer-window-entity-respiratory-rate-value = {$value}breaths/minute
health-analyzer-window-entity-spo2-value = {$value}%
health-analyzer-window-entity-etco2-value = {$value}mmHg
health-analyzer-window-entity-non-medical-reagents = [color=yellow]Patient has non-medical reagents in bloodstream.[/color]

wound-bone-death = [color=red]Patient's {$target} has systemic bone failure.[/color]
wound-internal-fracture = [color=red]Patient's {$target} has internal fractures.[/color]
wound-incision = [color=red]Patient's {$target} has open incision.[/color]
wound-clamped = [color=red]Patient's {$target} has clamped arteries.[/color]
wound-retracted = [color=red]Patient's {$target} has retracted skin.[/color]
wound-ribcage-open = [color=red]Patient's {$target} has open ribcage.[/color]
wound-arterial-bleeding = [color=red]Patient's {$target} has arterial bleeding.[/color]

health-analyzer-window-no-patient-damages = Patient has no injuries.

health-analyzer-status-tooltip =
    {"[bold]"}Alive[/bold]: The patient is alive and conscious.
    {"[bold]"}Critical[/bold]: The patient is unconscious and will die without intervention.
    {"[bold]"}Dead[/bold]: The patient is dead and will rot without intervention.

health-analyzer-blood-pressure-tooltip =
    A measure of how much blood is making it throughout the body.

health-analyzer-spo2-tooltip =
    A measure of how much oxygen or nitrogen is making it to the patient's body, compared to what the patient needs.

health-analyzer-heart-rate-tooltip =
    A measure of how fast the patient's heart is beating.

    The heartrate increases in response to inadequate Oxygen or Nitrogen.

health-analyzer-respiratory-rate-tooltip =
    A measure of how fast the patient is breathing.

    Breathing too fast can result in less air per breath, causing asphyxiation.

health-analyzer-etco2-tooltip =
    A measure of how much waste gas is being exhaled with each breath.

    Low ETCO2 can result in toxic waste gas buildup.

health-analyzer-heart-health-tooltip =
    A measure of the heart's integrity.

    It will decrease due to excessively high heartrate.

    Relevant metrics:
    {"[color=#7af396]"}Heart Rate[/color]: {$heartrate}bpm

health-analyzer-plain-temperature-tooltip =
    The patient's body temperature.

health-analyzer-cryostasis-temperature-tooltip =
    The patient's body temperature.

    This temperature has a cryostasis factor of {$factor}%.

health-analyzer-lung-health-tooltip =
    The patient's lung health.

    The lower this number, the more difficulty they have breathing.

    If the lung health is low, consider putting the patient on higher-pressure internals.

health-analyzer-blood-tooltip =
    The patient's blood volume.

health-analyzer-damage-tooltip =
    The patient's total accumulated injuries.

health-analyzer-brain-health-tooltip = { $dead ->
    [true] {-health-analyzer-brain-health-tooltip-dead}
   *[false] {-health-analyzer-brain-health-tooltip-alive(spo2: $spo2)}
    }

-health-analyzer-brain-health-tooltip-alive =
   Overall measure of brain health. Total brain death cannot be recovered from.

-health-analyzer-brain-health-tooltip-dead =
    The patient has 0% brain activity and is dead.
