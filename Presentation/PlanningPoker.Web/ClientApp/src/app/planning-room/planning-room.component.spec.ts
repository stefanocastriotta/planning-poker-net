import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlanningRoomComponent } from './planning-room.component';

describe('PlanningRoomComponent', () => {
  let component: PlanningRoomComponent;
  let fixture: ComponentFixture<PlanningRoomComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PlanningRoomComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlanningRoomComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
