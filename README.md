# Approximately Up Items Mod

MelonLoader mod for **Approximately Up Demo** that gives access to all items.
Based on https://github.com/DAMIOTF/ApproximatelyUP-MOD-MENU.

<p>
  <a href="#english">English</a> |
  <a href="#russian">Русский</a>
</p>

---

<a id="english"></a>

## English

### Features

- **F10** opens or closes the item window.
- **F9** sets item amounts to `99999` and fills the hotbar.
- The item window can set item amounts to `999` or `99999`.
- Clicking an item in the list puts it into the first hotbar slot.
- **Max item amounts + fill hotbar** fills hotbar slots 1-10.

### Requirements

- Windows.
- Approximately Up Demo.
- [MelonLoader](https://github.com/LavaGang/MelonLoader.Installer/releases/latest) installed for the game.

### Installation

1. Download and install [MelonLoader Installer](https://github.com/LavaGang/MelonLoader.Installer/releases/latest).
2. Select `ApproximatelyUp.exe` in your Approximately Up Demo game folder.
3. Start the game once, then close it.
4. Download the latest release archive of this mod.
5. Extract the archive into the Approximately Up Demo game folder.
6. Start the game.

### If the Mod Does Not Work After a Game Update

If **F9/F10 do nothing** and the MelonLoader log contains:

```text
[ERROR] No Support Module Loaded!
```

Do this:

1. Close the game.
2. Right-click `PatchUnityCoreModule.ps1` in the game folder.
3. Choose **Run with PowerShell**.
4. Start the game again.

If you only downloaded the `.dll`, download the full release archive. It includes `PatchUnityCoreModule.ps1`.

You only need to run this script when the mod stops working after a game update.

### Updating

- To update the mod, replace `Mods\ApproximatelyUpMOD.dll`.
- If the game was updated and the mod stops responding, run `PatchUnityCoreModule.ps1` once.

---

<a id="russian"></a>

## Русский

### Возможности

- **F10** открывает или закрывает окно предметов.
- **F9** выставляет количество предметов на `99999` и заполняет хотбар.
- В окне мода можно выставить количество предметов на `999` или `99999`.
- Нажатие на предмет в списке кладет его в первый слот хотбара.
- **Max item amounts + fill hotbar** заполняет слоты хотбара 1-10.

### Требования

- Windows.
- Approximately Up Demo.
- [MelonLoader](https://github.com/LavaGang/MelonLoader.Installer/releases/latest), установленный для игры.

### Установка

1. Скачайте и установите [MelonLoader Installer](https://github.com/LavaGang/MelonLoader.Installer/releases/latest).
2. В установщике выберите `ApproximatelyUp.exe` в папке игры Approximately Up Demo.
3. Запустите игру один раз, затем закройте ее.
4. Скачайте последний release-архив этого мода.
5. Распакуйте архив в папку игры Approximately Up Demo.
6. Запустите игру.

### Если Мод Не Работает После Обновления Игры

Если **F9/F10 ничего не делают**, а в логе MelonLoader есть:

```text
[ERROR] No Support Module Loaded!
```

Сделайте так:

1. Закройте игру.
2. Нажмите правой кнопкой по `PatchUnityCoreModule.ps1` в папке игры.
3. Выберите **Run with PowerShell**.
4. Запустите игру снова.

Если вы скачали только `.dll`, скачайте полный release-архив. В нем уже есть `PatchUnityCoreModule.ps1`.

Скрипт нужно запускать только тогда, когда мод перестал работать после обновления игры.

### Обновление

- Чтобы обновить мод, замените `Mods\ApproximatelyUpMOD.dll`.
- Если игра обновилась и мод перестал реагировать, один раз запустите `PatchUnityCoreModule.ps1`.
