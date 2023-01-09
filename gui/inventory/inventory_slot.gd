class_name InventorySlot
extends TextureRect

signal state_changed()
signal item_grabbed(grabbed_item)
signal item_released()

var display_name:String
var quantity:=0

var gui_root:Control
onready var item_texture:TextureRect=$CenterContainer/TextureRect
onready var label:Label=$Label
onready var popup:PopupPanel=$PopupPanel
onready var world_node:=get_tree().root.get_node("GameScreen/ViewportContainer/Viewport/World")

var _str_size:Vector2


func _ready():
	$PopupPanel/Label.text=display_name
	gui_root=get_tree().root.get_node("GameScreen/ViewportContainer/GUILayer/GUI")
	connect("item_grabbed",gui_root,"_on_item_grabbed")
	connect("item_released",gui_root,"_on_item_released")


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
			get_global_mouse_position()+Vector2(16,0),
			_str_size))
	else:
		popup.hide()


func _input(event:InputEvent):
	if event is InputEventMouseButton:
		var mb:=event as InputEventMouseButton
		if mb.pressed:
			if mb.button_index==BUTTON_LEFT and Rect2(Vector2.ZERO,rect_size).has_point(get_local_mouse_position()):
				if display_name=="":
					if gui_root.grabbed_item:
						# place
						set_item_info(gui_root.grabbed_item.display_name,gui_root.grabbed_item.texture,gui_root.grabbed_item.quantity)
						emit_signal("item_released")
						emit_signal("state_changed")
				else:
					var grabbed:GrabbedItem=preload("res://gui/inventory/grabbed_item.tscn").instance()
					grabbed.set_item_info(display_name,item_texture.texture,quantity)
					if gui_root.grabbed_item:
						if gui_root.grabbed_item.display_name==grabbed.display_name:
							# merge
							var max_stack:int=world_node.FindItemData(grabbed.display_name)["Stack"]
							print_debug(gui_root.grabbed_item.quantity)
							print_debug(quantity)
							if gui_root.grabbed_item.quantity+quantity<=max_stack:
								set_item_quantity(quantity+gui_root.grabbed_item.quantity)
								emit_signal("item_released")
							else:
								var diff:=max_stack-quantity
								set_item_quantity(quantity+diff)
								gui_root.grabbed_item.set_item_quantity(gui_root.grabbed_item.quantity-diff)
						else:
							# swap
							set_item_info(gui_root.grabbed_item.display_name,gui_root.grabbed_item.texture,gui_root.grabbed_item.quantity)
							emit_signal("item_released")
							emit_signal("item_grabbed",grabbed)
						emit_signal("state_changed")
					else:
						# grab
						emit_signal("item_grabbed",grabbed)
						emit_signal("state_changed")
						clear()


func set_item_quantity(n:int):
	quantity=n
	if n==0 or n==1:
		label.text=""
	else:
		label.text=str(n)
