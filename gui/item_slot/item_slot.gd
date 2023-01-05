tool
extends TextureRect

export var focused:=false setget set_focused
var item_name:String

onready var item_texture:TextureRect=$CenterContainer/TextureRect
onready var label:Label=$Label


func _ready():
	pass


func set_focused(f:bool):
	focused=f
	$BoldFrame.visible=focused


func _on_item_changed(tex:Texture,name:String):
	item_name=name
	item_texture.texture=tex


func _on_number_changed(n:int):
	if n==0:
		_on_item_changed(null,"")
		label.visible=false
	elif n==1:
		label.visible=false
	else:
		label.text=str(n)
