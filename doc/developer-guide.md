# **FUSE Repository Contract – Entwicklerdokumentation**

# *(IRepository, ExpressionTree, FieldPredicate, FieldOperators)*

 

Diese Dokumentation beschreibt den **FUSE-fx Repository Contract** für .NET – den generischen Schnittstellenvertrag, mit dem Sie Fachlogik sauber von Persistenztechnologien entkoppeln. Im Fokus stehen:

- die generische Schnittstelle      IRepository<TEntity, TKey>
- das Filtermodell      ExpressionTree inkl. FieldPredicate und FieldOperators
- Capabilities zur      Feature-Aushandlung mit einem Repository
- Sortierung, Paging,      Feldselektion und Massenoperationen
- zahlreiche C#-Beispiele (von      einfach bis komplex)

Quellen: Die Signaturen und Felder stammen direkt aus dem Repository Contract. Bitte beachten Sie, dass einige Generics/Typen im zitierten Raw-View verkürzt dargestellt sein können; die Semantik ist hier vollständig erläutert. [GitHub+4GitHub+4GitHub+4](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Ziel & Motivation

 

Der FUSE Repository Contract schafft eine **einheitliche, performante und technologieagnostische Zugriffsschicht** für Entitäten. Er grenzt sich positiv ab durch:

- **Konsequente      Trennung** von      Domänenlogik und Speichertechnologie
- **Portables      Filtermodell**      (ExpressionTree) ohne Technologiebindung
- **Klares      Capability-Modell**      zur Laufzeit-Aushandlung verfügbarer Features
- **Leichtgewichtige      Entity-Referenzen**,      Fieldbags und Massenoperationen

Damit lassen sich dieselben Business-Use-Cases z. B. gegen SQL, NoSQL, Filesysteme oder Web-APIs konsistent bedienen.

 

 

# **Schnellstart (Top-down)**

 

# 1) Repository beziehen & Fähigkeiten prüfen

#  



*GetOriginIdentity() liefert einen technischen Bezeichner (z. B. Server+Schema+Entity);* ***nicht*** *als UI-Label verwenden.* [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

#  

# 2) Einfache Suche mit ExpressionTree & Sortierung

 



*^ vor einem Feldnamen bedeutet* ***DESC****; ansonsten* ***ASC****. Sortierung erfolgt* ***vor*** *limit/skip.* [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

#  

# 3) Feldselektion (nur bestimmte Felder laden)

 



Fieldbags *sind Dictionaries mit Feldnamen als Keys.* (Die Rohsignaturen zeigen Dictionary[]; in der Praxis werden Dictionary<string, object> genutzt.) [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# 4) Schreiben: Add/Update – „Upsert“-Stile

 



Die genaue Semantik (z. B. ob externe Keys benötigt werden, ob Insert bei fehlendem Key möglich ist) richtet sich nach RequiresExternalKeys und ggf. weiterer Capabilities – siehe Kapitel **Capabilities & Verhalten**. [GitHub+1](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Repository Capabilities & Verhalten

 

RepositoryCapabilities ist ein Property-Bag, das Funktionen beschreibt, die eine konkrete Implementierung unterstützt. Wichtige Flags: [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/RepositoryCapabilities.cs)

- CanReadContent:      Entitäten/Feldinhalte können gelesen werden (sonst nur EntityRef).
- CanUpdateContent: Update von      Inhalten möglich.
- CanAddNewEntities: Neue      Entitäten können angelegt werden.
- CanDeleteEntities: Löschen      wird unterstützt.
- SupportsMassupdate:      Massenupdates per Keys/Filter/Expression.
- SupportsKeyUpdate:      Primärschlüssel können geändert werden.
- SupportsStringBasedSearchExpressions:      String-basierte Suchausdrücke sind erlaubt (siehe unten).
- RequiresExternalKeys: Ob beim      Einfügen der Key **extern** (vom Aufrufer) geliefert werden muss; falls false,      erzeugt der Store Keys selbst.

RepositoryCapabilities.All liefert eine Konfiguration mit allen Features aktiviert – primär als Referenz/Vorgabe gedacht. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/RepositoryCapabilities.cs)

 

# Filtermodell: ExpressionTree, FieldPredicate & FieldOperators

 

# ExpressionTree

 

ExpressionTree beschreibt eine **baumartige, technologieagnostische Filterlogik**. Kernelemente: [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/ExpressionTree.cs)

- MatchAll (bool): true = **AND**-Relation, false = **OR**-Relation auf dieser Ebene
- Negate (bool): Ergebnis      negieren
- Predicates (Liste): **atomare      Prädikate**      (Feldname ~ Wert)
- SubTree (Liste): **Unterausdrücke** (rekursiv)

 

Konstruktor-Shortcuts:

 



 

 

# Sonderfall Mehrfach-Prädikate pro Feld:

 

Enthält Predicates bei MatchAll = true mehrere Prädikate **mit demselben Feldnamen**, wird **implizit ein untergeordneter OR-Ausdruck** für dieses Feld gebildet („Field-lokales OR“). [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/ExpressionTree.cs)

Das ToString() dient Debuggingzwecken und gibt eine lesbare Formel zurück. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/ExpressionTree.cs)

 

# FieldOperators

 

Symbolische Operatoren, die in FieldPredicate.Operator verwendet werden: [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/FieldOperators.cs)

| **Operator** | **Numeric**    | **String**  | **Date**       | **Bool**    |
| ------------ | -------------- | ----------- | -------------- | ----------- |
| ==           | Equal          | Equal       | Equal          | Equal       |
| !=           | NotEqual       | NotEqual    | NotEqual       | NotEqual    |
| <            | Less           | *invalid*   | Less           | *invalid*   |
| <=           | LessOrEqual    | SubstringOf | LessOrEqual    | *invalid*   |
| >            | Greater        | *invalid*   | Greater        | *invalid*   |
| >=           | GreaterOrEqual | Contains    | GreaterOrEqual | *invalid*   |
| \|*          | *invalid*      | StartsWith  | *invalid*      | *invalid*   |
| *\|          | *invalid*      | EndsWith    | *invalid*      | *invalid*   |
| in           | *array-only    | *array-only | *array-only    | *array-only |

 

 

**Generisch (alle Datentypen):**

- == (Equal)
- != (NotEqual)
- in (In) – **Wert MUSS      ein Array** sein;      Match, wenn mindestens ein Array-Wert gleich ist

 

**Numerisch & Datum:**

- < (Less)
- <= (LessOrEqual)
- \> (Greater)
- \>= (GreaterOrEqual)

 

**String-spezifisch:**

- |* (StartsWith)
- *| (EndsWith)
- <= (SubstringOf) – *Hinweis:      Operatorzeichen wird auch für „LessOrEqual“ verwendet; die Bedeutung ist      datentypabhängig.*
- \>= (Contains) – *Hinweis:      Analog, datentypabhängig.*

Wichtig: Einige Operatoren sind **datentypabhängig überladen** (z. B. wird <= bei Strings als *SubstringOf* interpretiert). Für andere Datentypen „greifen“ die semantisch passenden Varianten oder es gibt **kein Match**. Details sind in den Operator-Kommentare vermerkt. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/FieldOperators.cs)

 

# FieldPredicate

 

Atomare Prädikate bestehen aus **Feldname**, **Operator** und **Wert**. Dazu gibt es Factory-Methoden: Equal, NotEqual, GreaterOrEqual, Greater, StartsWith, SubstringOf, Contains. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/FieldPredicate.cs)

 

FieldPredicate p1 = FieldPredicate.Equal("Age", 18);
 FieldPredicate p2 = FieldPredicate.StartsWith("Lastname", "Mil");
 FieldPredicate p3 = new FieldPredicate { FieldName = "Id", Operator = FieldOperators.In, Value = new string[] { "C-001", "C-007" } };

.NET (Core)-Hinweis: In der Referenz ist (bedingt) eine **serialisierte Wertablage** (ValueSerialized) für JSON vorgesehen, inkl. TryGetValue<T>(). Konkrete Implementierungen können dies verwenden, um Werte typstabil zu transportieren. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/FieldPredicate.cs)

 

# **API-Referenz: IRepository<TEntity, TKey>**

Die folgende Liste erläutert alle Methoden samt Verhalten. Details (Sortierung, Limit/Paging, Capabilities) sind direkt in den Signaturkommentaren hinterlegt. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Identität & Fähigkeiten

 

- **string      GetOriginIdentity()**           Technische      Herkunftskennung (z. B. Server/Schema/Entität). Kein UI-Label.
             *Verwendung:*      Logging, Telemetrie, Multiquellen-Routing. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

- **RepositoryCapabilities      GetCapabilities()**           Laufzeitfähigkeiten      des Repositories (siehe oben). [GitHub+1](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Lesen – Referenzen (leichte Objekte)

 

- **EntityRef[]      GetEntityRefs(ExpressionTree filter, string[] sortedBy, int limit = 100,      int skip = 0)**           Liefert      leichte Referenzen passender Entitäten. Unterstützt Sortierung,      Paging.
             *Use-Case:*      Ergebnislisten, Autosuggests, Cross-Lookups. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

- **EntityRef[]      GetEntityRefsBySearchExpression(string searchExpression, string[]      sortedBy, int limit = 100, int skip = 0)**           Wie oben,      aber via **String-Suchausdruck**. Nur, wenn SupportsStringBasedSearchExpressions =      true.
             *Hinweis:*      Die **Syntax**      des Stringausdrucks ist **Implementierungsdetail** der jeweiligen      Repository-Implementierung; für maximale Portabilität wird ExpressionTree      empfohlen. [GitHub+1](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

- **EntityRef[]      GetEntityRefsByKey(TKey[] keysToLoad)**           Lädt      Referenzen zu explizit genannten Schlüsseln. Nicht vorhandene Keys können      ignoriert werden. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Lesen – Entitäten (volle Objekte)

 

- **TEntity[]      GetEntities(ExpressionTree filter, string[] sortedBy, int limit = 100, int      skip = 0)**           Vollständige      Entitäten, sortiert und paginiert. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

- **TEntity[]      GetEntitiesBySearchExpression(string searchExpression, string[] sortedBy,      int limit = 100, int skip = 0)**           Wie oben,      via String-Suchausdruck; nur bei entsprechender Capability. [GitHub+1](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

- **TEntity[]      GetEntitiesByKey(TKey[] keysToLoad)**           Vollständige      Entitäten zu den angegebenen Keys. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Lesen – Field-Bags

 

- **Dictionary[]      GetEntityFields(ExpressionTree filter, string[] includedFieldNames,      string[] sortedBy, int limit = 100, int skip = 0)**           Lädt **nur      bestimmte Felder**      pro Entität – effizient bei großen Objekten.
             *Praxis:*      Dictionary<string, object>[]. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

- **Dictionary[]      GetEntityFieldsBySearchExpression(string searchExpression, string[]      includedFieldNames, string[] sortedBy, int limit = 100, int skip =      0)**           Feldselektion      via String-Suchausdruck. Capability erforderlich. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

- **Dictionary[]      GetEntityFieldsByKey(TKey[] keysToLoad, string[] includedFieldNames)**           Feldselektion      für explizite Keys. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Zählen & Existenz

 

- **int      CountAll()** –      Anzahl aller Entitäten. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **int      Count(ExpressionTree filter)** – Anzahl passend zum Filter. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **int      CountBySearchExpression(string searchExpression)** – Anzahl via Stringausdruck      (Capability nötig). [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **bool      ContainsKey(TKey key)** – Existenzprüfung. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Schreiben – Upsert/Update/Add

 

- **Dictionary      AddOrUpdateEntityFields(Dictionary fields)**           Upsert als      Fieldbags.
             **Wichtig:**      Verhalten hängt von RequiresExternalKeys und vorhandenem Key ab      (insert/update/skip). Rückgabe enthält **abweichende Felder** (z. B. normalisierte Werte),      oder null falls nicht anwendbar. **Den zurückgegebenen Key immer      auswerten!** [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **TEntity      AddOrUpdateEntity(TEntity entity)**           Upsert als      Entität. Rückgabe ist die **neue Entitätsversion** (mit impliziten Änderungen      wie Timestamps/RowVersions). [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **Dictionary      TryUpdateEntityFields(Dictionary fields)           Update**      bestimmter Felder für **genau eine** Entität (Key **muss** im Input enthalten sein). Rückgabe: **abweichende      Felder** oder null      falls nicht gefunden. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **TEntity      TryUpdateEntity(TEntity entity)**           Vollupdate      für **genau eine**      Entität (per Key). Rückgabe: **aktualisierte Entität** oder null. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **TKey      TryAddEntity(TEntity entity)**           Fügt eine      Entität hinzu und gibt den Key zurück (oder null, z. B. bei Duplikat). Ob      der Key **vorab** gesetzt sein muss, steuert RequiresExternalKeys. [GitHub+1](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Massenoperationen

 

Nur verfügbar, wenn SupportsMassupdate = true. Varianten für Key-Mengen, Filter und Stringausdrücke. **Key-Felder dürfen nicht upgedatet werden** – sonst Exception. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

- **TKey[]      MassupdateByKeys(TKey[] keysToUpdate, Dictionary fields)** – Update an Schlüsselmenge;      Rückgabe: aktualisierte Keys. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **TKey[]      Massupdate(ExpressionTree filter, Dictionary fields)** – Update per Filter. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **TKey[]      MassupdateBySearchExpression(string searchExpression, Dictionary fields)** – Update via Stringausdruck      (zusätzliche Capability erforderlich). [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Löschen & Key ändern

 

- **TKey[]      TryDeleteEntities(TKey[] keysToDelete)**           Löscht      Entitäten und gibt **nur die tatsächlich gelöschten Keys** zurück. Erfordert      CanDeleteEntities. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **bool      TryUpdateKey(TKey currentKey, TKey newKey)**           Ändert den      Primärschlüssel. Erfordert SupportsKeyUpdate. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Sortierung & Paging

 

- **Sortierung:** Array von Feldnamen, ^ als      DESC-Präfix (z. B. new[] { "^Age", "Lastname" }).
- **Reihenfolge:** Sortierung erfolgt **vor** limit/skip.
- **Paging:** limit (Anzahl), skip      (Offset).
             [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# String-basierte Suchausdrücke (optional)

 

Einige Methoden akzeptieren searchExpression als **String**. Dies ist **nur zulässig**, wenn SupportsStringBasedSearchExpressions = true.

**Wichtig:** Die **konkrete Syntax** der Suchausdrücke ist **Repository-spezifisch**. Für maximale Portabilität sollte bevorzugt ExpressionTree genutzt werden. [GitHub+1](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

 

# **Erweiterte Beispiele**

 

# Feld-lokales OR bei MatchAll = true

 



*Dieses Konstrukt entspricht „Category == 'A' OR Category == 'B'“ innerhalb eines sonstigen AND-Kontextes.* [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/ExpressionTree.cs)

#  

#  

# Negation & geschachtelte Ausdrücke

 



#  

# IN-Operator

 



*Beim Operator in* ***muss*** *Value ein Array sein.* [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/FieldOperators.cs)

 

# Teil-Feldupdate (TryUpdateEntityFields)

 



*Fehlt der Key im Input, wirft die Implementierung eine Exception.* [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Feldselektion mit Paging & stabile Sortierung

 



 

# Massupdate per Filter

 



*Semantik der Feldoperation (Ersetzen vs. Patch) ist Implementierungsdetail; viele Repositories interpretieren Werte als Zielwerte (Set).*

*Massupdate erfordert SupportsMassupdate = true.* [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Key-Änderung

 

// Rename primary key from "C-001" to "C-001A"
 bool moved = repo.TryUpdateKey("C-001", "C-001A");

*Nur erlaubt mit SupportsKeyUpdate = true.* [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Fehlerbehandlung & Edge-Cases

 

- **Keys &      Upserts:** Prüfen      Sie RequiresExternalKeys.

- - true: Insert erfordert **extern** gesetzten Key; Upsert ohne       Key kann übersprungen werden.
  - false: Store darf Key       erzeugen. In diesem Fall **Rückgabewert auf Key prüfen** (kann neu sein). [GitHub+1](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/RepositoryCapabilities.cs)

- **String-Suchausdrücke:** Nur verwenden, wenn      Capability aktiv. Syntax kann je nach Implementierung variieren (nicht      portabel). [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/RepositoryCapabilities.cs)

- **Massupdate:** Key-Felder **niemals** in fields angeben → sonst      Exception. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

- **TryUpdateEntityFields:** Key muss im Input enthalten      sein, ansonsten Exception. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

- **Löschen:** Rückgabe enthält **nur      erfolgreich gelöschte** Keys. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)

 

# Performance-Hinweise

 

- Verwenden Sie **Feldselektion** (GetEntityFields*) bei      Listen/Grids statt Vollentitäten.
- Nutzen Sie **Sortierung +      Paging**      deterministisch (stabile Sortierung mit sekundären Keys).
- Verwenden Sie **EntityRef** dort, wo nur      Schlüssel/Anzeigewerte gebraucht werden.
- Für Joins/Lookups zuerst **Key-Mengen      bestimmen**,      anschließend **ByKey-Methoden** verwenden.
- Prüfen Sie **Capabilities** einmalig und **zweigen** Sie Pfade (z. B.      Stringausdruck vs. ExpressionTree) ab.

 

 

 

# Zusammenfassung der Anforderungen (Kurzüberblick)

 

| **Bereich**           | **Muss** | **Details**                                                  |
| --------------------- | -------- | ------------------------------------------------------------ |
| Generisches Repo      | ✔️        | IRepository<TEntity,   TKey> zum Lesen/Schreiben/Löschen     |
| Capabilities          | ✔️        | Aushandlung zur   Laufzeit (Massupdate, Key-Update, Stringausdrücke, …) [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/RepositoryCapabilities.cs) |
| Filter                | ✔️        | ExpressionTree mit   FieldPredicate und FieldOperators [GitHub+2GitHub+2](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/ExpressionTree.cs) |
| Sortierung &   Paging | ✔️        | sortedBy mit ^ für   DESC; limit/skip vor Sortierung wirksam [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs) |
| Feldselektion         | ✔️        | Field-Bags   (Dictionaries) für effizientes Laden [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs) |
| Massenoperationen     | optional | Nur falls   SupportsMassupdate = true [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs) |
| String-Suchausdruck   | optional | Nur falls   SupportsStringBasedSearchExpressions = true [GitHub+1](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs) |

 

# Bottom-up: Wichtigste Artefakte im Überblick

 

- **IRepository<TEntity,      TKey>**:      Zentrale Schnittstelle für CRUD- und Massenoperationen sowie Zählungen. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/IRepository.cs)
- **RepositoryCapabilities**: Feature-Set einer konkreten      Implementierung (lesen, schreiben, massupdate, key update, string      expression, key policy). [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/RepositoryCapabilities.cs)
- **ExpressionTree**: Baumstruktur für Filter      (AND/OR, Negation, Predicates, SubTrees). Enthält Debug-ToString() und      Fabrikmethoden Empty/And/Or. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/ExpressionTree.cs)
- **FieldPredicate**: Atomare Bedingung      (Feld/Operator/Wert) inkl. praktischer Factory-Methoden. Optional      JSON-Serialization von Werten in .NET Core. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/FieldPredicate.cs)
- **FieldOperators**: Konstanten für Operatoren;      einige **datentypabhängig** (z. B. <= = *SubstringOf* für Strings). Enthält in für      Array-Matches. [GitHub](https://raw.githubusercontent.com/SmartStandards/FUSE-fx.RepositoryContract/refs/heads/master/dotnet/src/RepositoryContract/Logic/FieldOperators.cs)

![img](data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAdCAYAAACjbey/AAAABGdBTUEAALGPC/xhBQAAAAlwSFlzAAAOwgAADsIBFShKgAAAABp0RVh0U29mdHdhcmUAUGFpbnQuTkVUIHYzLjUuMTAw9HKhAAAAMUlEQVRIS2P4TyFgoFD//1ED/o+GATARjaYDaoQBAwPDf4rwaHYeTYmjuRGcCygukQAAKQla9U4a5wAAAABJRU5ErkJggg==)