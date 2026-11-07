"""Original SAS4-inspired low-poly enemy kit. Blender 5.2, FBX + editable sources.
All dimensions in metres. Front is -Y in Blender, +Z in Unity. No downloaded models.
"""
import bpy, math, os, random, json
from mathutils import Vector
ROOT=os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT=os.path.join(ROOT,'Assets','Enemies','Models')
SOURCE=os.path.dirname(os.path.abspath(__file__))
os.makedirs(OUT,exist_ok=True)
random.seed(17)
report=[]

def init():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    global mats, parts
    parts=[];mats={}
    colors={'Skin':(.57,.61,.47,1),'Muscle':(.47,.19,.16,1),'Mutant':(.26,.48,.43,1),'Cloth':(.14,.17,.2,1),'Dark':(.025,.028,.025,1),'Bone':(.65,.59,.37,1),'Metal':(.25,.28,.27,1),'Acid':(.38,.59,.09,1)}
    for n,c in colors.items():
        m=bpy.data.materials.new(n);m.diffuse_color=c;mats[n]=m

def ell(name, pos, scale, mat='Skin', seg=16, rings=10):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,radius=1,location=pos)
    o=bpy.context.object;o.name=name;o.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(mats[mat]);parts.append(o)
    for f in o.data.polygons:f.use_smooth=True
    return o

def link(name,a,b,r1,r2=None,mat='Skin',sides=12):
    a,b=Vector(a),Vector(b);v=b-a
    bpy.ops.mesh.primitive_cone_add(vertices=sides,radius1=r1,radius2=r2 if r2 is not None else r1,depth=v.length,location=(a+b)/2)
    o=bpy.context.object;o.name=name;o.rotation_euler=v.to_track_quat('Z','Y').to_euler()
    bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
    o.data.materials.append(mats[mat]);parts.append(o)
    for f in o.data.polygons:f.use_smooth=True
    return o

def box(name,pos,dim,mat,bevel=.015):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=name;o.dimensions=dim
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    o.data.materials.append(mats[mat]);parts.append(o)
    if bevel:
        mod=o.modifiers.new('Worn edge','BEVEL');mod.width=bevel;mod.segments=1;bpy.ops.object.modifier_apply(modifier=mod.name)
    return o

def maw_ring(size):
    verts=[];faces=[]
    for i in range(32):
        a=i*math.tau/32
        for j in range(8):
            b=j*math.tau/8;thickness=.055*(1+.15*math.sin(a*3))
            verts.append(((.30+math.cos(b)*thickness)*math.cos(a)*size,(-.35+math.sin(b)*thickness)*size,(1.70+(.25+math.cos(b)*thickness)*math.sin(a))*size))
    for i in range(32):
        for j in range(8):faces.append((i*8+j,((i+1)%32)*8+j,((i+1)%32)*8+(j+1)%8,i*8+(j+1)%8))
    mesh=bpy.data.meshes.new('Continuous fleshy lip');mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new('Maw lip',mesh);bpy.context.collection.objects.link(o);o.data.materials.append(mats['Muscle']);parts.append(o)
    return o

def join(objects,name):
    bpy.ops.object.select_all(action='DESELECT')
    for o in objects:o.select_set(True)
    bpy.context.view_layer.objects.active=objects[0]
    if len(objects)>1:bpy.ops.object.join()
    o=bpy.context.object;o.name=name
    bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
    return o

def uv(o,mat):
    bpy.context.view_layer.objects.active=o;bpy.ops.object.select_all(action='DESELECT');o.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.1,island_margin=.02);bpy.ops.object.mode_set(mode='OBJECT')
    quadrants={'Skin':(0,.5),'Muscle':(.5,.5),'Mutant':(0,0),'Cloth':(.5,0)}
    for p in o.data.polygons:
        n=o.data.materials[p.material_index].name
        if n in quadrants:
            off=quadrants[n]
            for li in p.loop_indices:
                old=o.data.uv_layers.active.data[li].uv.copy()
                o.data.uv_layers.active.data[li].uv=(off[0]+.012+old.x*.476,off[1]+.012+old.y*.476)

def distance_segment(p,a,b):
    d=b-a;t=max(0,min(1,(p-a).dot(d)/max(.0001,d.length_squared)))
    return (p-(a+d*t)).length

def rig_and_export(name,bones,rigid,clips,voxel=.035):
    # Fuse the skin masses to remove intersecting capsule seams, keeping deliberate accessories separate.
    skin=[o for o in parts if o not in rigid]
    body=join(skin,'Body')
    rem=body.modifiers.new('Unified anatomy','REMESH');rem.mode='VOXEL';rem.voxel_size=voxel;rem.use_smooth_shade=True
    bpy.ops.object.modifier_apply(modifier=rem.name)
    sm=body.modifiers.new('Skin relaxation','SMOOTH');sm.factor=.7;sm.iterations=3;bpy.ops.object.modifier_apply(modifier=sm.name)
    dec=body.modifiers.new('Low-end topology','DECIMATE');dec.ratio=.34;bpy.ops.object.modifier_apply(modifier=dec.name)
    # Remesh uses the first material, which is intentionally the base skin family.
    uv(body,'Skin')
    arm=bpy.data.armatures.new(name+'Skeleton');rig=bpy.data.objects.new(name+'Rig',arm);bpy.context.collection.objects.link(rig)
    bpy.context.view_layer.objects.active=rig;rig.select_set(True);body.select_set(False);bpy.ops.object.mode_set(mode='EDIT')
    for bn,(a,b,parent) in bones.items():
        bone=arm.edit_bones.new(bn);bone.head=a;bone.tail=b
        if parent:bone.parent=arm.edit_bones[parent]
    bpy.ops.object.mode_set(mode='OBJECT')
    for ob in [body]+list(rigid):
        if ob!=body:uv(ob,'Skin')
        groups={n:ob.vertex_groups.new(name=n) for n in bones}
        fixed=rigid.get(ob)
        for v in ob.data.vertices:
            if fixed: groups[fixed].add([v.index],1,'REPLACE');continue
            p=ob.matrix_world@v.co
            ds=sorted((distance_segment(p,Vector(a),Vector(b)),n) for n,(a,b,_) in bones.items() if n not in ('Root','Jaw'))[:2]
            weights=[1/max(.025,d)**4 for d,n in ds];total=sum(weights)
            for w,(_,n) in zip(weights,ds):groups[n].add([v.index],w/total,'REPLACE')
    mesh=join([body]+list(rigid),name+'Mesh');mesh.parent=rig
    mod=mesh.modifiers.new('Skeleton','ARMATURE');mod.object=rig
    for f in mesh.data.polygons:f.use_smooth=True
    scene=bpy.context.scene;scene.render.fps=30
    for clip,seconds,loop in clips:
        rig.animation_data_clear();frames=max(2,round(seconds*30))
        for pb in rig.pose.bones:pb.rotation_mode='XYZ'
        for frame in sorted(set(range(1,frames+2,3)) | {frames+1}):
            f=min(frame,frames+1);t=(f-1)/frames;phase=t*2*math.pi
            for pb in rig.pose.bones:pb.rotation_euler=(0,0,0);pb.location=(0,0,0);pb.scale=(1,1,1)
            def rot(b,x=0,y=0,z=0):
                if b in rig.pose.bones:rig.pose.bones[b].rotation_euler=tuple(math.radians(a) for a in (x,y,z))
            def loc(b,x=0,y=0,z=0):
                if b in rig.pose.bones:rig.pose.bones[b].location=(x,y,z)
            worm=name in ('Worm','PukeWorm')
            heavy=name in ('Bloater','Regurgitator')
            if worm:
                for i in range(4):rot('Segment'+str(i),0,0,math.sin(phase-i*.8)*(12 if clip=='Move' else 3))
                if clip=='Melee':rot('Segment0',-math.sin(math.pi*t)*35)
                if clip=='Death':loc('Root',0,-.12*t,0);rig.pose.bones['Root'].scale=(1,1-.5*t,1)
            elif clip=='Idle':
                rot('Spine',(7 if name=='Shambler' else 11 if name=='Sprinter' else 0)+math.sin(phase)*1.5);rot('Head',0,math.sin(phase)*2,math.sin(phase)*1.5)
                loc('Hips',0,math.sin(phase)*.012,0)
            elif clip=='Move':
                amp=17 if heavy else 40 if name=='Sprinter' else 23
                for side,offset in [('L',0),('R',math.pi)]:
                    a=math.sin(phase+offset)*(0.65 if name=='Shambler' and side=='R' else 1);rot('Thigh'+side,a*amp);rot('Shin'+side,max(0,-a)*(48 if name=='Sprinter' else 25));rot('Foot'+side,-a*9)
                    rot('UpperArm'+side,-a*(13 if heavy else 29),0,(-1 if side=='L' else 1)*6);rot('Forearm'+side,-12-max(0,a)*18)
                rot('Spine',5 if name=='Shambler' else 10 if name=='Sprinter' else 0,0,math.sin(phase)*3)
                loc('Hips',0,abs(math.sin(phase))*(.025 if heavy else .045),0)
                rot('Head',-3,math.sin(phase)*3)
            elif clip=='Melee':
                # Wind-up, decisive strike, then recovery. The preview/controller controls the impact window.
                swing=math.sin(math.pi*min(t/.68,1))
                rot('UpperArmR',-100*swing,0,-12*swing);rot('ForearmR',-35*swing)
                rot('UpperArmL',-65*swing if name=='Regurgitator' else -25*swing,0,12*swing)
                rot('Spine',18*math.sin(math.pi*t),0,-9*math.sin(math.pi*t));rot('Head',-10*swing)
            elif clip in ('Spit','Vomit'):
                pulse=math.sin(math.pi*t);rot('Spine',-12*pulse if t<.4 else 24*pulse);rot('Head',-18*pulse);rot('Jaw',32*pulse)
                for side in ['L','R']:rot('UpperArm'+side,-25*pulse,0,(1 if side=='R' else -1)*12*pulse)
            elif clip=='Hit':rot('Spine',-12*math.sin(math.pi*t));rot('Head',8*math.sin(math.pi*t))
            elif clip=='Death':
                fall=min(1,max(0,(t-.1)/.7));rot('Root',-86*fall,0,8*fall);loc('Root',0,.12*fall,0)
                rot('UpperArmL',-22*fall,0,-25*fall);rot('UpperArmR',12*fall,0,30*fall);rot('ShinL',32*fall)
            for pb in rig.pose.bones:
                pb.keyframe_insert('rotation_euler',frame=f,group=pb.name);pb.keyframe_insert('location',frame=f,group=pb.name);pb.keyframe_insert('scale',frame=f,group=pb.name)
        action=rig.animation_data.action;action.name=name+'_'+clip;action.use_fake_user=True
    rig.animation_data_create();rig.animation_data.action=bpy.data.actions[name+'_Idle']
    rig.animation_data.action_slot=rig.animation_data.action.slots[0]
    scene.frame_set(1)
    for pb in rig.pose.bones:pb.rotation_euler=(0,0,0);pb.location=(0,0,0);pb.scale=(1,1,1)
    bpy.ops.object.select_all(action='DESELECT');rig.select_set(True);mesh.select_set(True);bpy.context.view_layer.objects.active=rig
    scene.frame_start=1;scene.frame_end=61
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(SOURCE,name+'.blend'))
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,name+'.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},axis_forward='-Z',axis_up='Y',add_leaf_bones=False,use_armature_deform_only=True,bake_anim=True,bake_anim_use_nla_strips=False,bake_anim_use_all_actions=True,bake_anim_simplify_factor=0,mesh_smooth_type='FACE')
    tris=sum(len(p.vertices)-2 for p in mesh.data.polygons)
    report.append({'name':name,'triangles':tris,'bones':len(bones),'clips':[c[0] for c in clips]})
    print('ENEMY_EXPORT',name,tris,'triangles',len(bones),'bones')

def humanoid(name):
    init();rigid={}
    heavy=name in ('Bloater','Regurgitator');boss=name=='Regurgitator';spr=name=='Sprinter';spit=name=='Spitter'
    size=1.72 if boss else 1.25 if heavy else 1.03 if spit else 1
    width=1.7 if boss else 1.65 if heavy else .85 if spr else 1
    skin='Mutant' if boss or spit else 'Muscle' if spr else 'Skin'
    def p(x,y,z):return (x*size,y*size,z*size)
    def e(label,pos,scale,material=skin,bone=None):
        o=ell(label,p(*pos),tuple(v*size for v in scale),material)
        if bone:rigid[o]=bone
        return o
    bones={'Root':(p(0,0,0),p(0,0,.2),None),'Hips':(p(0,0,.88),p(0,0,1.1),'Root'),'Spine':(p(0,0,1.1),p(0,0,1.34),'Hips'),'Chest':(p(0,0,1.34),p(0,0,1.54),'Spine'),'Head':(p(0,-.06,1.54),p(0,-.11,1.82),'Chest'),'Jaw':(p(0,-.12,1.63),p(0,-.22,1.57),'Head')}
    e('Pelvis',(0,.015,.94),(.22*width,.16,.2))
    e('Waist',(0,0,1.12),(.19*width,.15,.23))
    e('Rib cage',(0,.015,1.36),(.29*width,.19 if not heavy else .30,.27))
    e('Back muscles',(0,.10,1.38),(.28*width,.15,.22))
    if heavy:e('Distended belly',(0,-.11,1.11),(.37*width,.39,.43))
    if spit:
        e('Acid throat',(0,-.15,1.52),(.17,.16,.22),'Mutant')
        e('Back acid gland',(0,.19,1.44),(.26,.21,.25),'Mutant')
        e('Throat bile sac',(.055,-.18,1.55),(.10,.075,.12),'Acid','Chest')
        e('Shoulder bile sac',(-.23,.12,1.52),(.13,.11,.15),'Acid','Chest')
    if not boss:
        e('Neck',(0,-.045,1.59),(.105,.10,.12))
        e('Skull',(0,-.09,1.76),(.145,.13,.19))
        e('Jaw',(0,-.16,1.64),(.11,.10,.08),skin,'Jaw')
        for x in [-.062,.062]:
            e('Sunken socket',(x,-.202,1.79),(.034,.013,.024),'Dark','Head')
            e('Dull eye',(x,-.215,1.79),(.004,.004,.005),'Bone','Head')
            brow=e('Brow ridge',(x,-.192,1.817),(.055,.027,.022),skin,'Head');brow.rotation_euler.y=(.16 if x>0 else -.16)
        e('Nose',(0,-.221,1.73),(.023,.016,.038),skin,'Head')
        e('Open mouth',(0,-.244,1.665),(.061,.012,.032),'Dark','Jaw')
        for i,x in enumerate([-.044,-.014,.033]):
            o=box('Broken teeth',p(x,-.259,1.684),p(.011,.010,.017+i*.004),'Bone',0);rigid[o]='Jaw'
    else:
        # The Regurgitator has no conventional head: a recessed red maw sits between its shoulders.
        e('Maw back',(0,-.12,1.66),(.36,.24,.28),skin)
        e('Maw cavity',(0,-.35,1.70),(.29,.04,.24),'Dark','Head')
        rigid[maw_ring(size)]='Head'
        for i in range(16):
            a=i*math.tau/16;x=math.cos(a)*.30;z=1.70+math.sin(a)*.25
            o=link('Maw tooth',p(x*.92,-.425,1.70+(z-1.70)*.92),p(x*.60,-.435,1.70+(z-1.70)*.60),.028*size,.003*size,'Bone',6);rigid[o]='Head'
        e('Tongue',(0,-.402,1.58),(.16,.018,.045),'Muscle','Head')
    for side,sgn in [('L',1),('R',-1)]:
        shoulder=p(sgn*.29*width,0,1.45);elbow=p(sgn*(.42*width+.08),-.025,1.08);wrist=p(sgn*(.47*width+.1),-.13,.76);hand=p(sgn*(.48*width+.1),-.16,.63)
        hip=p(sgn*.15*width,.005,.94);knee=p(sgn*.18*width,-.04,.51);ankle=p(sgn*.19*width,.015,.12);toe=p(sgn*.19*width,-.19,.08)
        bones.update({'UpperArm'+side:(shoulder,elbow,'Chest'),'Forearm'+side:(elbow,wrist,'UpperArm'+side),'Hand'+side:(wrist,hand,'Forearm'+side),'Thigh'+side:(hip,knee,'Hips'),'Shin'+side:(knee,ankle,'Thigh'+side),'Foot'+side:(ankle,toe,'Shin'+side)})
        for label,a,b,ra,rb in [('Arm',shoulder,elbow,.18 if heavy else .105,.14 if heavy else .08),('Forearm',elbow,wrist,.15 if heavy else .08,.12 if heavy else .06),('Thigh',hip,knee,.18 if heavy else .13,.12 if heavy else .085),('Shin',knee,ankle,.12 if heavy else .085,.07)]:
            link(label,a,b,ra*size,rb*size,skin)
            ell(label+' joint',a,(ra*size,ra*size,ra*size),skin)
        ell('Hand',(Vector(wrist)+Vector(hand))/2,(.12*size if heavy else .072*size,.075*size,.12*size),skin)
        for i in range(3):
            a=Vector(hand)+Vector(((i-1)*.045*size,0,0));b=a+Vector((0,-.045*size,-.11*size))
            o=link('Finger',a,b,.024*size,.013*size,skin,6);rigid[o]='Hand'+side
        ell('Foot',(Vector(ankle)+Vector(toe))/2,(.105*size,.19*size,.09*size),skin)
        if not spr:
            o=link('Torn trousers',hip,Vector(knee)+Vector((0,0,.05*size)),(.19 if heavy else .145)*size,.12*size,'Cloth');rigid[o]='Thigh'+side
        if name=='Shambler':
            o=link('Torn sleeve',shoulder,Vector(shoulder).lerp(Vector(elbow),.5),.125*size,.11*size,'Cloth');rigid[o]='UpperArm'+side
        if heavy:
            e('Shoulder bulk',(sgn*.32*width,.02,1.46),(.25,.24,.24),skin)
        if name=='Bloater' and side=='R':
            center=Vector(hand)+Vector((0,0,-.20*size))
            o=link('Cleaver grip',hand,center,.037*size,mat='Dark',sides=8);rigid[o]='HandR'
            o=box('Cleaver blade',center+Vector((-.12*size,0,-.12*size)),(.32*size,.045*size,.25*size),'Metal',.01);rigid[o]='HandR'
    if name=='Shambler':
        e('Tattered shirt',(0,.02,1.33),(.295,.207,.26),'Cloth','Chest')
        e('Shirt waist',(0,.025,1.12),(.22,.178,.20),'Cloth','Spine')
        e('Torn shirt tail',(-.08,.005,.99),(.15,.18,.14),'Cloth','Hips')
        # Broken front opening and exposed clavicle distinguish clothing from an armored character.
        e('Exposed chest',(0,-.19,1.43),(.115,.026,.15),'Skin','Chest')
    if not spr:e('Trouser waist',(0,.035,.91),(.26*width,.19,.16),'Cloth','Hips')
    if heavy:
        for i in range(5):
            e('Infected nodule',((-.25+i*.12),-.40,1.12+math.sin(i*2)*.17),(.055,.04,.07),'Muscle','Spine')
    clips=[('Idle',2,True),('Move',1.6 if heavy else .7 if spr else 1.2,True),('Melee',1.6 if heavy else .9,False),('Hit',.4,False),('Death',1.5,False)]
    if spit:clips.append(('Spit',1.3,False))
    if boss:clips.append(('Vomit',2.4,False))
    rig_and_export(name,bones,rigid,clips,.053 if boss else .04 if heavy else .026)

def worm(name):
    init();size=1.3 if name=='PukeWorm' else 1;skin='Mutant' if name=='PukeWorm' else 'Skin';rigid={}
    bones={'Root':((0,0,0),(0,0,.1),None)}
    for i in range(4):
        y=-.34+i*.22;bone='Segment'+str(i)
        bones[bone]=((0,y,.18*size),(0,y+.22,.18*size),'Root' if i==0 else 'Segment'+str(i-1))
        ell('Larval segment',(0,y+.08,.18*size),((.18-i*.025)*size,.21*size,.15*size),skin)
        for side in [-1,1]:
            o=link('Larval bristle',(side*.12*size,y,.16*size),(side*.21*size,y-.04,.06),.016,.004,'Bone',5);rigid[o]=bone
    o=ell('Larval maw',(0,-.49,.19*size),(.11*size,.02,.09*size),'Dark');rigid[o]='Segment0'
    rig_and_export(name,bones,rigid,[('Idle',1.3,True),('Move',.55,True),('Melee',.65,False),('Hit',.3,False),('Death',.8,False)],.023)

for name in ['Shambler','Sprinter','Spitter','Bloater','Regurgitator']:humanoid(name)
for name in ['Worm','PukeWorm']:worm(name)
with open(os.path.join(SOURCE,'model-report.json'),'w') as f:json.dump(report,f,indent=2)
print('ENEMY_KIT_COMPLETE',json.dumps(report))
