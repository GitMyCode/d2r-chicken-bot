

// https://github.com/noah-/d2bs/blob/master/Constants.h
public enum Stat : short {
  STAT_STRENGTH =  0,  // str
  STAT_ENERGY =  1,    // energy
  STAT_DEXTERITY =  2, // dexterity
  STAT_VITALITY =  3,  // vitality
  STAT_STATPOINTSLEFT =  4,
  STAT_SKILLPOINTSLEFT =  5,
  STAT_HP =  6,          // life
  STAT_MAXHP =  7,       // max life
  STAT_MANA =  8,        // mana
  STAT_MAXMANA =  9,     // max mana
  STAT_STAMINA =  10,    // stamina
  STAT_MAXSTAMINA =  11, // max stamina
  STAT_LEVEL =  12,      // level
  STAT_EXP =  13,        // experience
  STAT_GOLD =  14,       // gold
  STAT_GOLDBANK =  15,   // stash gold
  STAT_ENHANCEDDEFENSE =  16,
  STAT_ENHANCEDDAMAGEMAX =  17, // njipStats["itemmaxdamagepercent"]=[17,0];
  STAT_ENHANCEDDAMAGEMIN =  18, // njipStats["itemmindamagepercent"]=[18,0];	njipStats["enhanceddamage"]=[18,0];
  STAT_TOBLOCK =  20,           // to block
  STAT_LASTEXP =  29,
  STAT_NEXTEXP =  30,
  STAT_DAMAGEREDUCTION =  36,      // damage reduction
  STAT_MAGICDAMAGEREDUCTION =  35, // magic damage reduction
  STAT_MAGICRESIST =  37,          // magic resist
  STAT_MAXMAGICRESIST =  38,       // max magic resist
  STAT_FIRERESIST =  39,           // fire resist
  STAT_MAXFIRERESIST =  40,        // max fire resist
  STAT_LIGHTNINGRESIST =  41,      // lightning resist
  STAT_MAXLIGHTNINGRESIST =  42,   // max lightning resist
  STAT_COLDRESIST =  43,           // cold resist
  STAT_MAXCOLDRESIST =  44,        // max cold resist
  STAT_POISONRESIST =  45,         // poison resist
  STAT_MAXPOISONRESIST =  46,      // max poison resist
  STAT_LIFELEECH =  60,            // Life Leech
  STAT_MANALEECH =  62,            // Mana Leech
  STAT_VELOCITYPERCENT =  67,      // effective run/walk
  STAT_AMMOQUANTITY =  70,         // ammo quantity(arrow/bolt/throwing)
  STAT_DURABILITY =  72,           // item durability
  STAT_MAXDURABILITY =  73,        // max item durability
  STAT_GOLDFIND =  79,             // Gold find (GF)
  STAT_MAGICFIND =  80,            // magic find (MF)
  STAT_ITEMLEVELREQ =  92,
  STAT_IAS =  93,                     // IAS
  STAT_FASTERRUNWALK =  96,           // faster run/walk
  STAT_FASTERHITRECOVERY =  99,       // faster hit recovery
  STAT_FASTERBLOCK =  102,            // faster block rate
  STAT_FASTERCAST =  105,             // faster cast rate
  STAT_POISONLENGTHREDUCTION =  110,  // Poison length reduction
  STAT_OPENWOUNDS =  135,             // Open Wounds
  STAT_CRUSHINGBLOW =  136,           // crushing blow
  STAT_DEADLYSTRIKE =  141,           // deadly strike
  STAT_FIREABSORBPERCENT =  142,      // fire absorb %
  STAT_FIREABSORB =  143,             // fire absorb
  STAT_LIGHTNINGABSORBPERCENT =  144, // lightning absorb %
  STAT_LIGHTNINGABSORB =  145,        // lightning absorb
  STAT_COLDABSORBPERCENT =  148,      // cold absorb %
  STAT_COLDABSORB =  149,             // cold absorb
  STAT_SLOW =  150,                   // slow %
}