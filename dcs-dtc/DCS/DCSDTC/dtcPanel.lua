local dxgui = require('dxgui')
local DialogLoader = require("DialogLoader")
local lfs = require("lfs")

--[[
    DTC Preset Browser Panel
    In-game panel for browsing and uploading presets without Alt-Tabbing.

    Communication:
      DCS → C# (port 43000):
        {"request_presets":"<aircraft>"}  -- panel opened, requesting preset list
        {"select_preset":"<name>","aircraft":"<aircraft>","upload":"true"}  -- upload requested
        {"show_dtc_app":"1"}  -- Open DTC app button pressed

      C# → DCS (port 43001):
        preset_list:[{"name":"<n>","waypoints":<w>,"radios":"<r>"},...]
]]

local DTCPresetPanel = {
    width = 500,
    height = 600,
    dialog = nil,
    visible = false,

    presets = {},        -- list of {name, waypoints, radios}
    selectedIndex = 1,
    aircraftType = "",

    callback = nil,
}

function DTCPresetPanel:init(eventCallback)
    self.callback = eventCallback

    local screenWidth, screenHeight = dxgui.GetScreenSize()
    local x = math.floor((screenWidth / 2) - (self.width / 2))
    local y = math.floor((screenHeight / 2) - (self.height / 2))

    self.dialog = DialogLoader.spawnDialogFromFile(
        lfs.writedir() .. "Scripts\\DCSDTC\\dtcPanel.dlg")
    self.dialog:setVisible(true)
    self.dialog:setBounds(x, y, self.width, self.height)

    -- Register hotkey to toggle panel (Ctrl+Shift+P)
    self.dialog:addHotKeyCallback("Ctrl+Shift+p", function()
        eventCallback:showHidePresetPanel()
    end)

    -- Button callbacks
    self.dialog.btnClose:addMouseUpCallback(function()
        self:hide()
    end)

    self.dialog.btnPrev:addMouseUpCallback(function()
        if #self.presets == 0 then return end
        self.selectedIndex = self.selectedIndex - 1
        if self.selectedIndex < 1 then self.selectedIndex = #self.presets end
        self:refreshDisplay()
    end)

    self.dialog.btnNext:addMouseUpCallback(function()
        if #self.presets == 0 then return end
        self.selectedIndex = self.selectedIndex + 1
        if self.selectedIndex > #self.presets then self.selectedIndex = 1 end
        self:refreshDisplay()
    end)

    self.dialog.btnUpload:addMouseUpCallback(function()
        eventCallback:uploadSelectedPreset(self)
    end)

    self.dialog.btnOpenApp:addMouseUpCallback(function()
        eventCallback:sendDataToDTC('{"show_dtc_app":"1"}')
    end)

    self:hide()
end

function DTCPresetPanel:show(eventCallback)
    self.aircraftType = eventCallback:getCurrentAircraftType() or ""
    self.dialog.lblAircraft:setText("DTC: " .. self.aircraftType)
    self.dialog.lblStatus:setText("Loading presets...")
    self.presets = {}
    self.selectedIndex = 1
    self:refreshDisplay()

    self.dialog:setHasCursor(true)
    self.dialog:setSize(self.width, self.height)
    self.visible = true

    -- Request preset list from C# app
    eventCallback:sendDataToDTC('{"request_presets":"' .. self.aircraftType .. '"}')
end

function DTCPresetPanel:hide()
    if self.dialog then
        self.dialog:setHasCursor(false)
        self.dialog:setSize(0, 0)
    end
    self.visible = false
end

function DTCPresetPanel:receivePresetList(jsonStr)
    self.presets = {}
    -- Parse JSON array without a JSON library.
    -- C# serializes in fixed key order: {"name":"...","waypoints":N,"radios":"..."}
    for name, waypoints, radios in string.gmatch(jsonStr,
        '"name":"([^"]+)","waypoints":(%d+),"radios":"([^"]*)"') do
        table.insert(self.presets, {
            name = name,
            waypoints = tonumber(waypoints) or 0,
            radios = radios
        })
    end

    if #self.presets == 0 then
        self.dialog.lblStatus:setText("No presets found.")
    else
        self.selectedIndex = 1
        self.dialog.lblStatus:setText(#self.presets .. " preset(s) loaded")
    end

    self:refreshDisplay()
end

function DTCPresetPanel:refreshDisplay()
    local text = ""
    if #self.presets == 0 then
        text = "  (no presets)\n"
    else
        for i, p in ipairs(self.presets) do
            local marker = (i == self.selectedIndex) and ">> " or "   "
            local radiosStr = (p.radios ~= "" and p.radios ~= nil) and ("  [" .. p.radios .. "]") or ""
            local wptStr = (p.waypoints > 0) and (" (" .. p.waypoints .. " wpts)") or ""
            text = text .. marker .. p.name .. wptStr .. radiosStr .. "\n"
        end
    end
    self.dialog.presetList:setText(text)
end

return DTCPresetPanel
