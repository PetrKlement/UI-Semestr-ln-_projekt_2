# UI-Semestrální projekt 2
Celý program byl zpracován v jazyce C#. 
- Úkolem projektu bylo najít nejkratší cestu mezi městy v Tasmánii s využitím logiky genetického algoritmu. Hodnoty vzdáleností mezi nimi jsou v dokumentu Distance.xlsx. 
************************************
# Postup
Jednotlivé kroky genetického algoritmu v našem případě jsou:
- Inicializace – vytvoření první generace
- Vyhodnocení – určení celkové vzdálenosti pro jednotlivé trasy
- Selekce – kratší trasy mají větší šanci na zachování
- Křížení – promísení částí dvou tras
- Mutace – náhodné změny
- Opakování – proces se opakuje od bodu vyhodnocení
***************
# Spuštění aplikace:
Windows:
- Je třeba mít soubor UI2.exe, který je součástí odevzdané práce
- Je třeba mít program, který je schopen ho spustit. V našem případě například IDE Visual studio
- Je třeba mít přiložený soubor Distance.xlsx uložený na disku D ( D:\ Distance.xlsx). Pokud by ho chtěl mít uživatel uložen jinde, je třeba upravit cestu ve třídě HledaniCest na 32. řádku.
***************

