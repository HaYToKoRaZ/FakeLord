#define AppName "FakeLord"
#ifndef AppVersion
  #define AppVersion "1.2.0"
#endif

[Setup]
AppId={{5E4A8D9B-1C3F-4E7A-9281-DF14B7A83E22}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} v{#AppVersion}
AppPublisher=HaYTo
AppPublisherURL=https://github.com/HaYToKoRaZ/FakeLord
AppSupportURL=https://github.com/HaYToKoRaZ/FakeLord/issues
AppUpdatesURL=https://github.com/HaYToKoRaZ/FakeLord/releases
AppContact=korazhayto@gmail.com
DefaultDirName={localappdata}\Programs\FakeLord
DefaultGroupName=FakeLord
AllowNoIcons=yes
DisableProgramGroupPage=no
UninstallDisplayIcon={app}\FakelordUI.exe
SetupIconFile=FakelordUI\app.ico
UninstallIconFile=FakelordUI\app.ico
OutputDir=..\release
OutputBaseFilename=FakeLord-Windows-Setup-v{#AppVersion}
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest
WizardStyle=modern
Compression=lzma2/ultra64
SolidCompression=yes
ChangesAssociations=no

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[CustomMessages]
turkish.WelcomeLinks=Bağlantılar: <a href="https://github.com/HaYToKoRaZ/FakeLord">GitHub Deposu</a>  •  <a href="https://x.com/HaYTo">X (@HaYTo)</a>
english.WelcomeLinks=Explore: <a href="https://github.com/HaYToKoRaZ/FakeLord">GitHub Repository</a>  •  <a href="https://x.com/HaYTo">X (@HaYTo)</a>
turkish.DeleteUserDataPrompt=Kullanıcı ayarları ve önbellek (config.ini, loglar vb.) kalıcı olarak silinsin mi?
english.DeleteUserDataPrompt=Permanently delete user configuration and cache (config.ini, logs, etc.)?

[Tasks]
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur / Create a desktop shortcut"; GroupDescription: "Ek Kısayollar / Additional shortcuts:"

[Files]
Source: "..\installer-source\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\FakelordUI.exe"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\FakelordUI.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\FakelordUI.exe"; Description: "FakeLord uygulamasını başlat / Launch FakeLord"; Flags: postinstall nowait skipifsilent

[UninstallDelete]
Type: dirifempty; Name: "{app}"
Type: dirifempty; Name: "{localappdata}\Programs\FakeLord"

[Code]
var
  WelcomeLinksLabel: TNewLinkLabel;

procedure WelcomeLinksClick(Sender: TObject; const Link: String; LinkType: TSysLinkType);
var
  ErrorCode: Integer;
begin
  if LinkType = sltURL then
  begin
    if not ShellExec('open', Link, '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode) then
      MsgBox(SysErrorMessage(ErrorCode), mbError, MB_OK);
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  AppDir: String;
begin
  if CurUninstallStep = usUninstall then
  begin
    AppDir := ExpandConstant('{app}');
    if (not UninstallSilent) and FileExists(AppDir + '\config.ini') then
    begin
      if MsgBox(ExpandConstant('{cm:DeleteUserDataPrompt}'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
      begin
        DeleteFile(AppDir + '\config.ini');
        DelTree(AppDir + '\logs', True, True, True);
      end;
    end;
  end;
end;

procedure InitializeWizard;
begin
  WelcomeLinksLabel := TNewLinkLabel.Create(WizardForm);
  WelcomeLinksLabel.Parent := WizardForm.WelcomePage;
  WelcomeLinksLabel.Caption := ExpandConstant('{cm:WelcomeLinks}');
  WelcomeLinksLabel.OnLinkClick := @WelcomeLinksClick;
  WelcomeLinksLabel.Left := WizardForm.WelcomeLabel2.Left;
  WelcomeLinksLabel.Top := WizardForm.WelcomeLabel2.Top + WizardForm.WelcomeLabel2.Height + ScaleY(12);
  WelcomeLinksLabel.Width := WizardForm.WelcomeLabel2.Width;
  WelcomeLinksLabel.Height := ScaleY(24);
  WelcomeLinksLabel.Anchors := [akLeft, akRight, akTop];
  WelcomeLinksLabel.UseVisualStyle := True;
end;
