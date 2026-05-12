# -*- coding: utf-8 -*-
"""
Generates 4 tall images (one per category): Energy, Overclock, Armor, Weapons.
Each subroutine: Energy, Complexity, Name, Description, Effects. Uses English localization.
Requires: pip install Pillow
"""
from PIL import Image, ImageDraw, ImageFont
import os

# ============== НАСТРОЙКИ (меняй здесь для быстрой правки) ==============
CONFIG = {
    # Размер и отступы
    "img_w": 900,
    "padding": 24,
    "card_pad": 16,
    "gap": 12,
    # Высоты строк (px)
    "line_height_title": 28,
    "line_height_body": 20,
    "line_height_effect": 18,
    "icon_line_h": 18,
    # Блок с эмодзи и цифрами
    "icon_col_w": 56,
    "icon_row_h": 36,
    "icon_block_offset_down": 8,   # на сколько px опустить блок от верха карточки
    "baseline_from_icon_top": 14,  # базовая линия первой строки от icon_top
    "gap_number_to_energy_emoji": 5,
    "gap_number_to_complexity_emoji": 5,
    "energy_emoji_offset_down": 2,
    "complexity_emoji_offset_down": 5,
    "complexity_number_offset_down": 2,
    # Текст: запас по ширине (сколько символов вычитать из макс. длины строки)
    "text_margin_chars": 3,
    "effects_bullet_indent_px": 20,
    # Заголовок страницы
    "header_height": 72,
}
# ========================================================================

# Category: (internal_key, display_name, list of (sortKey_lo, sortKey_hi) ranges)
CATEGORIES = [
    ("energy", "Energy", [(300, 305)]),
    ("overclock", "Overclock", [(10, 100), (150, 158)]),
    ("armor", "Armor", [(100, 150)]),
    ("weapons", "Weapons", [(200, 300)]),
]

# Data: defName, label, description, effects[], biostatMet (energy), biostatCpx (complexity), sortKey (category_order)
# biostatMet: negative = consumes energy, positive = provides energy
SUBROUTINES = [
    # === ENERGY (sort 300+) ===
    ("ASE_DoubleReactorCapacity", "double reactor capacity", "Engineers increased the reactor capacity, but this slightly affects movement speed.", [], 8, 3, 300),
    ("ASE_EMPGenerator", "EMP reactor", "This experimental reactor produces a decent amount of energy, but its core periodically provokes EMP bursts that can negatively affect other androids and mechanisms.", ["Sometimes emits EMP"], 12, 6, 301),
    ("ASE_ExperimentalMechanoidReactor", "experimental mechanoid reactor", "Further increasing the reactor's energy efficiency led to an unexpected effect - its high frequency attracts other mechanoids.", ["Sometimes attracts hostile mechanoids"], 32, 5, 302),
    ("ASE_ExperimentalQuantumReactor", "experimental quantum reactor", "An increased-power reactor, but the reformatting time is tripled.", ["Reformatting time x3"], 12, 5, 302),
    ("ASE_VanometricReactor", "vanometric reactor", "An unstable prototype capable of generating an incredible amount of energy at the cost of an equally incredible amount of waste.", ["Significantly increased waste production"], 32, 7, 303),
    ("ASE_HandmadeEnergyBooster", "handmade reactor", "Hand-assembled and extremely fragile experimental reactor provides incredible power efficiency and work speed, but the android cannot be repaired after damage.", ["Cannot be repaired"], 32, 8, 304),
    # === OVERCLOCK (10-157) ===
    ("ASE_Overdrive_T1", "overdrive T1", "The android receives a slight optimization in motor circuits. Runs faster, but the battery drains quickly.", ["+1.0 move speed"], -2, 4, 10),
    ("ASE_Overdrive_T2", "overdrive T2", "Overclocking of stepper motor protocols. The android moves even faster, but is unable to carry heavy items.", ["+2.0 move speed", "-40 carrying capacity"], -4, 4, 11),
    ("ASE_Overdrive_T3", "overdrive T3", "The android moves at the limit of its capabilities, but its carrying capacity in this configuration is minimal.", [], -8, 6, 12),
    ("ASE_ProductivityProtocol_T1", "productivity protocol T1", "Work speed is slightly increased at the cost of higher energy consumption.", ["+40% global work speed"], -4, 4, 30),
    ("ASE_ProductivityProtocol_T2", "productivity protocol T2", "Work speed is significantly increased at the cost of even higher energy consumption.", ["+80% global work speed"], -8, 4, 31),
    ("ASE_ProductivityProtocol_T3", "productivity protocol T3", "Incredible productivity boost, but the cost is the inability to work efficiently at high temperatures.", ["The lower the temperature, the higher the work speed. Each degree below 20°C grants +2% global work speed, each degree above 20°C grants -2% global work speed."], -12, 6, 32),
    ("ASE_CargoHolder_T1", "cargo holder upgrade T1", "High-capacity cargo compartment.", ["+40 carrying capacity", "+50 mass carry capacity"], -1, 3, 40),
    ("ASE_CargoHolder_T2", "cargo holder upgrade T2", "Doubled-capacity cargo compartment, but the android's movement speed is slightly reduced.", ["+70 carrying capacity", "+100 mass carry capacity", "-1.0 move speed"], -4, 4, 41),
    ("ASE_CargoHolder_T3", "cargo holder upgrade T3", "Maximum carrying capacity, but significant speed reduction.", ["+100 carrying capacity", "+200 mass carry capacity", "-3.0 move speed"], -8, 6, 42),
    ("ASE_ResearchCore_T1", "research core T1", "Improved search and data systematization algorithms increase research speed.", ["+30% research speed"], -2, 3, 50),
    ("ASE_ResearchCore_T2", "research core T2", "Expanded memory blocks for even greater research acceleration.", ["+60% research speed"], -4, 4, 51),
    ("ASE_ResearchCore_T3", "research core T3", "Computational blocks operate at the limit, generating enormous amounts of heat and requiring additional cooling.", ["Heats surrounding area"], -6, 6, 52),
    ("ASE_MiningSubroutine_T1", "mining subroutine T1", "Rock structure analysis increases mining efficiency.", ["+20% Mining yield"], -2, 3, 60),
    ("ASE_MiningSubroutine_T2", "mining subroutine T2", "Improved sensors provide even more resources.", ["+40% Mining yield"], -4, 4, 61),
    ("ASE_DeepMiningSensors", "deep mining sensors", "Specialized sensors for deep mining of rare resources.", ["10% chance to extract additional resources"], -16, 6, 63),
    ("ASE_DrillingSpecialist", "drilling specialist", "Doubled drilling speed thanks to abandoning standard speech protocols.", ["Hearing loss and inability to communicate"], -8, 5, 64),
    ("ASE_FieldMedic", "field medic", "Emergency protocols for accelerated treatment of wounded fighters directly on the battlefield. Fast, but not very high quality.", ["+200% medical tend speed", "-90% medical tend quality"], -6, 4, 70),
    ("ASE_SurgicalUnit", "surgical unit", "Special model designed for use in elite clinics. Equipment fragility prevents the android from being used for heavy work.", ["99.9% surgery success rate", "+300% operation speed", "cannot build/mine/carry"], -8, 6, 71),
    ("ASE_AgroDrone", "agro-drone", "Optimized for harvesting crops and cutting down trees.", ["+70% plant harvest efficiency", "+70% cutting speed"], -4, 3, 80),
    ("ASE_NarcoticHarvester", "narcotic harvester", "Thanks to improved motor skills, ideal for growing illegal crops, but has much lower durability compared to standard models.", ["+200% narcotic crop yield", "+300% incoming damage"], -2, 4, 81),
    ("ASE_ShredderCore", "shredder core", "Specialized mechanoid processing unit that occasionally finds additional components in them.", ["50% chance to get bonus materials"], -8, 5, 83),
    ("ASE_ButcherCore", "butcher core", "This protocol ignores sanitary standards and butchers carcasses at double speed.", ["+100% butchery speed", "+100% butchery efficiency", "+10000% food poisoning chance when cooking food"], -4, 3, 84),
    ("ASE_SmelterCore", "smelter core", "Special protocol for metal smelting, allows faster smelting and occasionally obtaining rare components.", ["30% chance to get bonus materials"], -4, 4, 85),
    ("ASE_DiplomaticCore", "diplomatic core", "Android diplomat for trade and negotiations. To build trust with trading partners, it technically cannot use ranged weapons.", ["Cannot use ranged weapon"], -4, 3, 150),
    ("ASE_SuppressionUnit", "suppression unit", "Specialization for capturing prisoners and suppressing riots, but built-in blades slightly reduce movement speed.", ["+50% Arrest Success Chance", "+100% Suppression Power", "-20% Move Speed"], -4, 3, 151),
    ("ASE_PropagandaBroadcaster", "propaganda broadcaster", "Thanks to its huge built-in speakers, can influence the opinions of others, but the equipment slightly reduces movement speed.", ["+200% Social Impact", "+200% Ideoligion Conversion Power", "-20% Move Speed"], -6, 3, 152),
    ("ASE_ThermalRegulator_T1", "thermal regulator T1", "Extended comfortable temperature range.", ["ComfyTemperatureMin -40°C", "ComfyTemperatureMax +40°C"], -3, 3, 153),
    ("ASE_ThermalRegulator_T2", "thermal regulator T2", "Maximum comfortable temperature range.", ["ComfyTemperatureMin -80°C", "ComfyTemperatureMax +80°C"], -6, 3, 154),
    ("ASE_ConstructionMatrix", "construction matrix", "The android is designed for heavy work on construction sites in new, unexplored worlds.", ["+100% Construction Speed", "+100% Smoothing Speed"], -5, 3, 155),
    ("ASE_DecentralizedMind", "decentralized mind", "Distributed computing allows even load distribution across all android CPUs present on the map, increasing their efficiency.", ["+10% global work speed per android with this gene on the map"], -4, 5, 156),
    ("ASE_ResourceScanner", "resource scanner", "Built-in metal detector that sometimes marks previously unknown deposits of valuable materials.", ["Once per day reveals a random underground deposit"], -6, 4, 157),
    # === ARMOR (100-121) ===
    ("ASE_Heavy_Plating_T1", "heavy plating T1", "The android is covered with additional armor plates. Stronger - but noticeably slower.", ["Incoming damage ×0.8", "move speed ×0.5", "-10% global work speed"], -4, 3, 100),
    ("ASE_Heavy_Plating_T2", "heavy plating T2", "Thick armor plates turn the android into a walking tank, sacrificing most of its mobility.", ["Incoming damage ×0.5", "move speed ×0.3", "-20% global work speed"], -8, 5, 101),
    ("ASE_Heavy_Plating_T3", "heavy plating T3", "Armor protection has been brought to incredible levels. Almost invulnerable, but barely able to move or work.", ["Incoming damage ×0.2", "move speed ×0.1", "-40% global work speed"], -20, 8, 102),
    ("ASE_ReactiveArmor", "reactive armor", "Reactive armor that damages attackers in melee combat (reflects 100% of melee damage back to attackers, but the android also takes damage). Warning: in case of critical damage, the android may explode!", ["Melee attacks on the android damage both the attacker and the android"], -8, 6, 103),
    ("ASE_DeflectorSystem", "deflector system", "Experimental reflective panels that reflect bullets back at attackers. Trigger approximately 50% of the time.", ["50% chance to reflect shots back at attacker", "flammability = +500%"], -16, 7, 104),
    ("ASE_AntiInsectoidPlates_T1", "anti-insectoid plates", "Damn insectoids got everyone! Additional layers of experimental composite armor will help better withstand clashes with these creatures.", ["Additional armor with 80 durability that works against melee damage", "Can be restored (depends on crafting skill)"], -4, 3, 110),
    ("ASE_AntiInsectoidPlates_T2", "double anti-insectoid plates", "Damn insectoids really got everyone! Additional layers of experimental composite armor will help better withstand clashes with these creatures.", ["Additional armor with 160 durability that works against melee damage", "Can be restored (depends on crafting skill)"], -8, 4, 111),
    ("ASE_AntiInsectoidPlates_T3", "quadro anti-insectoid plates", "Damn insectoids got everyone so badly that engineers took extreme measures! Additional layers of experimental composite armor will help withstand a clash with an entire army of these creatures!", ["Additional armor with 300 durability that works against melee damage", "Can be restored (depends on crafting skill)"], -16, 5, 112),
    ("ASE_AutoRepairModule", "auto-repair module", "One of the most expensive and coveted technologies in the rim worlds, regenerative nano-machines restore any damage (even in combat), but consume a fabulous amount of energy", ["Automatic damage restoration", "Lost limbs require more time to restore"], -30, 10, 113),
    ("ASE_SilverOverlay", "silver overlay", "A special silver coating has been applied to the android's armor. Makes the android noticeably stronger.", ["+40 to all three types of armor (sharp, blunt, heat)"], -10, 4, 114),
    ("ASE_GoldenOverlay", "golden overlay", "A special gold coating has been applied to the android's armor. Makes the android even stronger, but greatly increases its cost and attracts unwanted attention.", ["Market value +20 000"], -15, 6, 115),
    ("ASE_DiamondOverlay", "diamond overlay", "A special diamond particle coating has been applied to the android's armor. It makes the android much stronger, but due to the cost of materials, half the galaxy will hunt for such androids.", ["Daily increases the total colony value"], -25, 5, 118),
    ("ASE_WallProtocol", "wall protocol", "The android is designed specifically for protecting settlements in aggressive worlds. Useless for attacking, but it will hold the line forever in defense. Due to huge size and low mobility, unable to travel in caravans.", ["Cannot use auto-repair", "Cannot participate in caravans", "Cannot perform any work"], -30, 5, 119),
    ("ASE_Resilience", "resilience", "Each blow against the android permanently increases its armor by 0.1%.", ["Each blow against the android permanently increases its armor by 0.1%"], -4, 4, 120),
    ("ASE_ToxicDischarge", "toxic discharge", "When taking damage, the android releases a toxic cloud that damages everyone nearby. The system has 5 charges that restore over time.", ["Toxic protection system"], -8, 5, 121),
    # === WEAPONS (200+) ===
    ("ASE_BerserkerProtocol", "berserker protocol", "A protocol created for public entertainment: the more damaged the android is, the more ferociously it fights. Very popular in underground arenas.", ["Increases damage when injured. Effect starts at <80% HP"], -8, 5, 200),
    ("ASE_BerserkerProtocol_T2", "berserker protocol T2", "Further development of the protocol created for public entertainment: the more damaged the android is, the more ferociously it fights. Due to imperfect technology, it may enter uncontrollable rage and become a threat to everything living within 1 kilometer (including your colonists).", ["Each 1% of missing health increases melee attack speed by 2%", "Chance to enter uncontrollable rage"], -8, 6, 200.5),
    ("ASE_SniperOptics", "sniper optics", "Improved optics significantly increase weapon range, but the android itself becomes extremely vulnerable.", ["+50% weapon range", "+200% incoming damage"], -2, 4, 202),
    ("ASE_TitanFist", "titan fist", "The android strikes much slower, but puts all its power into each blow.", ["melee damage ×5", "move speed -50%"], -10, 6, 202),
    ("ASE_UnarmoredEvasion", "unarmored evasion", "Thanks to additional servos, the android dodges melee attacks more easily. The fewer items worn by the android, the more effective.", ["+15 melee & ranged dodge chance per empty apparel slot"], -4, 4, 203),
    ("ASE_ScrapWarrior", "scrap warrior", "Thanks to cheap materials, the android is not afraid of damage and deals increased damage. The lower the android's cost, the greater the bonus.", ["Additional damage when android cost is less than 2000 silver, up to +50% at cost up to 250 silver"], -6, 5, 205),
    ("ASE_AutoFeedSystem", "auto-feed system", "Built-in hand-mounted loading system that allows the android to make up to 5 shots with instant aiming and ×5 firing speed. The system recharges over time.", ["Loading system for 5 shots"], -10, 7, 206),
    ("ASE_HeatedCircuits", "heated circuits", "The reactor's heat dissipation system is connected to the android's fists, allowing it to inflict burns with melee attacks", ["Each strike applies burns", "30% chance to ignite enemy"], -5, 4, 207),
    ("ASE_PiercingAlgorithms", "piercing algorithms", "The android can find weak spots in the enemy's armor, but cannot protect its own.", ["+0.50 melee armor penetration", "+30% incoming damage"], -7, 5, 208),
    ("ASE_PerfectAimProtocol", "perfect aim protocol", "Improved aiming protocol forces the android to aim for an eternity, but the hit will be perfect.", ["+300% aiming time", "+99 shooting accuracy"], -8, 6, 209),
    ("ASE_SiegeSpecialist", "siege specialist", "Improved geometric calculation block allows quick and accurate mortar fire, but such androids are often equipped with minimally acceptable armor.", ["Mortar miss radius ×0.1", "+50% incoming damage"], -8, 6, 211),
    ("ASE_MeleeOverdrive_T1", "melee overdrive T1", "Android created specifically for foggy and rainy biomes. Cannot use ranged weapons, but fights well in melee.", ["+80% melee damage", "+50% armor penetration", "+40% melee hit chance", "cannot use ranged weapon"], -8, 6, 213),
    ("ASE_MeleeOverdrive_T2", "melee overdrive T2", "Android created specifically for foggy and rainy biomes with many hostile creatures. Cannot use ranged weapons, but fights excellently in melee.", ["+150% melee damage", "+100% armor penetration", "+90% melee hit chance", "cannot use ranged weapon"], -16, 8, 214),
    ("ASE_CryoStrikeProtocol", "cryo-strike protocol", "Refrigerant from the cooling system is redirected to the android's fists: each strike significantly cools the target.", ["Cooling strikes (with chance to apply hypothermia)"], -7, 5, 215),
    ("ASE_TacticalNetwork", "tactical network", "Real-time tactical data exchange allows androids to fight more effectively.", ["Per android with this gene on the map: +5% accuracy, +5% melee dodge, +5% ranged dodge, +5% melee damage"], -6, 6, 220),
    ("ASE_CombatDataLogger", "combat data logger", "With each defeated enemy, the android accumulates vast amounts of data about its target, allowing it to become even more effective.", ["+1% damage per enemy killed (permanent)"], -6, 4, 221),
    ("ASE_BarrageProtocol", "barrage protocol", "The android spends less time aiming, showering the enemy with a hail of bullets, but accuracy leaves much to be desired.", ["Ranged attack speed ×5"], -2, 3, 222),
]

def wrap_text(draw, text, font, max_width):
    lines = []
    words = text.split()
    current = []
    for w in words:
        test = " ".join(current + [w])
        b = draw.textbbox((0, 0), test, font=font)
        if b[2] - b[0] <= max_width:
            current.append(w)
        else:
            if current:
                lines.append(" ".join(current))
            current = [w]
    if current:
        lines.append(" ".join(current))
    return lines

def main():
    cfg = CONFIG
    img_w = cfg["img_w"]
    padding = cfg["padding"]
    card_pad = cfg["card_pad"]
    line_height_title = cfg["line_height_title"]
    line_height_body = cfg["line_height_body"]
    line_height_effect = cfg["line_height_effect"]
    icon_col_w = cfg["icon_col_w"]
    gap = cfg["gap"]
    icon_row_h = cfg["icon_row_h"]

    font_paths = [
        "C:/Windows/Fonts/segoeui.ttf",
        "C:/Windows/Fonts/arial.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
    ]
    font_title = font_body = font_effect = font_icon_number = None
    font_emoji = None
    for p in font_paths:
        if os.path.isfile(p):
            try:
                font_title = ImageFont.truetype(p, 20)
                font_body = ImageFont.truetype(p, 14)
                font_effect = ImageFont.truetype(p, 12)
                font_icon_number = ImageFont.truetype(p, 16)
                break
            except Exception:
                pass
    if font_title is None:
        font_title = ImageFont.load_default()
        font_body = font_effect = font_icon_number = font_title
    # Emoji font (Windows)
    emoji_paths = ["C:/Windows/Fonts/seguiemj.ttf", "C:/Windows/Fonts/SegoeUIEmoji.ttf"]
    for p in emoji_paths:
        if os.path.isfile(p):
            try:
                font_emoji = ImageFont.truetype(p, 18)
                break
            except Exception:
                pass
    if font_emoji is None:
        font_emoji = font_icon_number
    # Font for ⚡ and ⚙: try Emoji then Symbol (PIL needs a font that has these glyphs)
    font_symbol = None
    for p in ["C:/Windows/Fonts/seguiemj.ttf", "C:/Windows/Fonts/seguisym.ttf"]:
        if os.path.isfile(p):
            try:
                font_symbol = ImageFont.truetype(p, 16)
                break
            except Exception:
                pass
    if font_symbol is None:
        font_symbol = font_icon_number

    content_w = img_w - 2 * padding
    text_w = content_w - icon_col_w - gap

    energy_positive = (100, 220, 140)
    energy_negative = (255, 140, 100)
    cpx_color = (140, 180, 255)
    card_bg = (38, 42, 55)
    card_border = (60, 68, 90)
    text_title = (240, 248, 255)
    text_desc = (200, 210, 230)
    text_effect = (170, 185, 210)
    try:
        tfont = ImageFont.truetype("C:/Windows/Fonts/segoeui.ttf", 26) if os.path.isfile("C:/Windows/Fonts/segoeui.ttf") else font_title
    except Exception:
        tfont = font_title

    draw_temp = ImageDraw.Draw(Image.new("RGB", (1, 1)))
    n_margin = cfg["text_margin_chars"]
    b_m = draw_temp.textbbox((0, 0), "W" * n_margin, font=font_body)
    text_w = text_w - (b_m[2] - b_m[0])
    effects_indent = cfg["effects_bullet_indent_px"]

    for cat_key, cat_name, ranges in CATEGORIES:
        items = [s for s in SUBROUTINES if any(lo <= s[6] < hi for lo, hi in ranges)]
        if not items:
            continue

        card_heights = []
        for defname, label, desc, effects, energy, cpx, _ in items:
            h = card_pad * 2
            text_h = line_height_title
            desc_lines = wrap_text(draw_temp, desc, font_body, text_w)
            text_h += len(desc_lines) * line_height_body
            if effects:
                text_h += line_height_effect
                for e in effects:
                    elines = wrap_text(draw_temp, "• " + e, font_effect, text_w - effects_indent)
                    text_h += len(elines) * line_height_effect
            text_h += card_pad
            ch = card_pad + max(icon_row_h, text_h) + card_pad
            card_heights.append(ch)

        header_h = cfg["header_height"]
        total_h = padding * 2 + header_h + padding + sum(card_heights) + (len(items) - 1) * gap
        img = Image.new("RGB", (img_w, int(total_h)), color=(28, 30, 38))
        draw = ImageDraw.Draw(img)

        draw.rectangle([0, 0, img_w, header_h], fill=(45, 52, 70))
        draw.text((padding, 18), f"Android Subroutines Expanded — {cat_name}", fill=(220, 230, 255), font=tfont)
        draw.text((padding, 46), "Energy | Complexity | Name | Description | Effects", fill=(160, 170, 200), font=font_effect)
        y = header_h + padding

        for i, (defname, label, desc, effects, energy, cpx, _) in enumerate(items):
            ch = card_heights[i]
            draw.rounded_rectangle([padding, y, img_w - padding, y + ch], radius=8, fill=card_bg, outline=card_border, width=1)
            cx = padding + card_pad
            line_h = cfg["icon_line_h"]
            icon_top = y + card_pad + cfg["icon_block_offset_down"]
            baseline1 = icon_top + cfg["baseline_from_icon_top"]
            baseline2 = icon_top + cfg["baseline_from_icon_top"] + line_h
            g_energy = cfg["gap_number_to_energy_emoji"]
            g_cpx = cfg["gap_number_to_complexity_emoji"]
            off_energy_emoji = cfg["energy_emoji_offset_down"]
            off_cpx_emoji = cfg["complexity_emoji_offset_down"]
            off_cpx_num = cfg["complexity_number_offset_down"]

            # Energy: number first (no + or -), then gap, then ⚡
            en_str = str(abs(energy)) if energy != 0 else "0"
            en_color = energy_positive if energy > 0 else (energy_negative if energy < 0 else text_effect)
            draw.text((cx, baseline1), en_str, fill=en_color, font=font_icon_number, anchor="lb")
            b_en = draw.textbbox((0, 0), en_str, font=font_icon_number, anchor="lb")
            energy_right = cx + (b_en[2] - b_en[0])
            draw.text((energy_right + g_energy, baseline1 + off_energy_emoji), "\u26a1", fill=(255, 220, 100), font=font_symbol, anchor="lb")
            # Complexity: number right-aligned under energy, then gap, then ⚙
            draw.text((energy_right, baseline2 + off_cpx_num), str(cpx), fill=cpx_color, font=font_icon_number, anchor="rb")
            b_sym = draw.textbbox((0, 0), "\u2699", font=font_symbol, anchor="lb")
            draw.text((energy_right + g_cpx, baseline2 + off_cpx_emoji), "\u2699", fill=(180, 200, 255), font=font_symbol, anchor="lb")
            cx += icon_col_w + gap
            cy = y + card_pad

            # Title, description, effects (title case)
            draw.text((cx, cy), label.title(), fill=text_title, font=font_title)
            cy += line_height_title
            for line in wrap_text(draw, desc, font_body, text_w):
                draw.text((cx, cy), line, fill=text_desc, font=font_body)
                cy += line_height_body
            if effects:
                cy += 4
                for e in effects:
                    for line in wrap_text(draw, "• " + e, font_effect, text_w - effects_indent):
                        draw.text((cx, cy), line, fill=text_effect, font=font_effect)
                        cy += line_height_effect

            y += ch + gap

        out_path = os.path.join(os.path.dirname(__file__), f"Subroutines_{cat_name}.png")
        img.save(out_path)
        print(f"Saved: {out_path}  ({img_w} x {int(total_h)} px, {len(items)} items)")

if __name__ == "__main__":
    main()
