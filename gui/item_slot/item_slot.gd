tool
extends TextureRect

export var focused:=false setget set_focused


func _ready():
	pass


func set_focused(f:bool):
	focused=f
	$BoldFrame.visible=focused
