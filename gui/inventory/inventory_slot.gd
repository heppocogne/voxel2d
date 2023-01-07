class_name InventorySlot
extends TextureRect

signal item_grabbed(grabbed_item)

var display_name:String
var quantity:=0

onready var item_texture:TextureRect=$CenterContainer/TextureRect
onready var label:Label=$Label
onready var popup:PopupPanel=$PopupPanel

var _str_size:Vector2


func _ready():
	$PopupPanel/Label.text=display_name


func clear():
	set_item_info("",null,0)


func set_item_info(item_name:String,texture:Texture,n:int):
	display_name=item_name
	$PopupPanel/Label.text=display_name
	_str_size=$PopupPanel/Label.get_font("font").get_string_size(display_name)
	item_texture.texture=texture
	set_item_quantity(n)


func _process(_delta:float):
	if Rect2(Vector2.ZERO,rect_size).has_point(get_local_mouse_position()) and display_name!="":
		popup.popup(Rect2(
			get_global_mouse_position(),
			_str_size))
	else:
		popup.hide()


func _input(event:InputEvent):
	if event is InputEventMouseButton:
		var mb:=event as InputEventMouseButton
		if mb.pressed:
			if mb.button_index==BUTTON_LEFT and Rect2(Vector2.ZERO,rect_size).has_point(get_local_mouse_position()):
				print_debug("pressed")
#				var grabbed:GrabbedItem=preload("res://gui/inventory/grabbed_item.tscn").instance()
#				grabbed.set_item_info(display_name,item_texture.texture,quantity)
#				emit_signal("item_grabbed",grabbed)
#				clear()


func set_item_quantity(n:int):
	quantity=n
	if n==0 or n==1:
		label.text=""
	else:
		label.text=str(n)
