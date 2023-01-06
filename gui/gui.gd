extends Control

export var player_nodepath:NodePath

onready var margin_container:Container=$MarginContainer
onready var vbox_container:Container=$MarginContainer/VBoxContainer
onready var hotbar:Container=$MarginContainer/VBoxContainer/VBoxContainer/Hotbar


func _ready():
	get_tree().root.connect("size_changed", self, "_on_Window_size_changed")
	var player_node:KinematicBody2D=get_node(player_nodepath)
	player_node.get_node("Inventory").connect("StateChanged",hotbar,"_on_PlayerInvetory_state_changed")
	hotbar.connect("selected_slot_changed",player_node,"OnHotbarStateChanged")
	_on_Window_size_changed()


func _on_Window_size_changed():
	margin_container.rect_size=OS.window_size
	vbox_container.rect_size=OS.window_size
