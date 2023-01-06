tool
extends TextureRect

export var focused:=false setget set_focused
var display_name:String

onready var item_texture:TextureRect=$CenterContainer/TextureRect
onready var label:Label=$Label


func _ready():
	pass


func set_focused(f:bool):
	focused=f
	$BoldFrame.visible=focused


func clear():
	set_item_info("",null,0)


func set_item_info(item_name:String,texture:Texture,n:int):
	display_name=item_name
	item_texture.texture=texture
	set_item_quantity(n)


func set_item_quantity(n:int):
	if n==0 or n==1:
		label.text=""
	else:
		label.text=str(n)
