extends PopupDialog

signal savedata_selected(name)

onready var load_button:Button=$MarginContainer/VBoxContainer/HBoxContainer/Load
var ref_current:Button


func _ready():
	var nothing:Control=$MarginContainer/VBoxContainer/ScrollContainer/VBoxContainer/CenterContainer
	var vbox:VBoxContainer=$MarginContainer/VBoxContainer/ScrollContainer/VBoxContainer
	var dir:=Directory.new()
	if dir.open("user://worlds")==OK:
		dir.list_dir_begin(true,true)
		var file_name:=dir.get_next()
		while file_name!="":
			var savedata_path:="user://worlds/"+file_name+"/"
			if (
					dir.file_exists(savedata_path+"world")
					and dir.file_exists(savedata_path+"screenshot.png")
					and dir.dir_exists(savedata_path+"chunks")
			):
				nothing.visible=false
				var savedata:Control=preload("res://gui/main/save_data.tscn").instance()
				vbox.add_child(savedata)
				var screenshot_texture:=ImageTexture.new()
				var screenshot_image:=Image.new()
				screenshot_image.load(savedata_path+"screenshot.png")
				screenshot_texture.create_from_image(screenshot_image)
				savedata.get_node("HBoxContainer/CenterContainer/Screenshot").texture=screenshot_texture
				savedata.get_node("HBoxContainer/WorldName").text=file_name
				savedata.connect("toggled",self,"_on_SaveData_toggled",[savedata])
			file_name=dir.get_next()
		dir.list_dir_end()


func _on_SaveData_toggled(pressed:bool,ref_self:Button):
	if pressed:
		if ref_current:
			ref_current.pressed=false
		ref_current=ref_self
		emit_signal("savedata_selected",ref_current.get_node("HBoxContainer/WorldName").text)
	else:
		ref_current=null
		emit_signal("savedata_selected","")


func _on_Cancel_pressed():
	hide()


func _on_LoadWorld_savedata_selected(name:String):
	if name=="":
		load_button.disabled=true
	else:
		load_button.disabled=false


func _on_Load_pressed():
	var game_screen:Control=preload("res://gameplay/game_screen.tscn").instance()
	get_tree().root.add_child(game_screen)
	var world:=game_screen.get_node("ViewportContainer/Viewport/World")
	world.WorldName=ref_current.get_node("HBoxContainer/WorldName").text
	world.LoadWorld()
	game_screen.get_node("GUILayer/GUI").setup()
	get_tree().root.get_node("Main").queue_free()
