import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteProductBacklogItemDialogComponent } from './delete-product-backlog-item-dialog.component';

describe('DeleteProductBacklogItemDialogComponent', () => {
  let component: DeleteProductBacklogItemDialogComponent;
  let fixture: ComponentFixture<DeleteProductBacklogItemDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DeleteProductBacklogItemDialogComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeleteProductBacklogItemDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
